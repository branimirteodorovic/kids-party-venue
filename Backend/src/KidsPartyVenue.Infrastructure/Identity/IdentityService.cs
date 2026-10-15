using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Users;
using KidsPartyVenue.Infrastructure.Authentication;
using KidsPartyVenue.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace KidsPartyVenue.Infrastructure.Identity;

internal sealed class IdentityService(
    IDateTimeProvider dateTimeProvider,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    TokenProvider tokenProvider,
    IOptions<JwtOptions> jwtOptions,
    KidsPartyVenueDbContext dbContext,
    ILogger<IdentityService> logger) : IIdentityService
{
    public async Task<Result<Guid>> RegisterAsync(string email, string password, string fullName, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = email.Trim(),
            UserName = email.Trim(),
            FullName = fullName.Trim(),
            CreatedAtUtc = dateTimeProvider.UtcNow
        };

        IdentityResult createUserResult = await userManager.CreateAsync(user, password);
        if (!createUserResult.Succeeded)
        {
            return Result.Failure<Guid>(MapCreateUserErrors(createUserResult.Errors));
        }

        IdentityResult roleResult = await userManager.AddToRoleAsync(user, Role.Parent);
        if (!roleResult.Succeeded)
        {
            logger.LogError(
                "Assigning role {Role} to {UserId} failed: {Errors}",
                Role.Parent,
                user.Id,
                roleResult.Errors.Select(e => e.Code));

            // Compensate: don't leave a user without a role behind
            IdentityResult deleteResult = await userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                logger.LogError(
                    "Deleting {UserId} after failed role assignment failed: {Errors}",
                    user.Id,
                    deleteResult.Errors.Select(e => e.Code));
            }

            return Result.Failure<Guid>(UserErrors.RoleAssignmentFailed);
        }

        return Result.Success(user.Id);
    }

    public async Task<Result<AuthTokens>> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            return Result.Failure<AuthTokens>(UserErrors.InvalidCredentials);
        }

        SignInResult signIn = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (signIn.IsLockedOut)
        {
            return Result.Failure<AuthTokens>(UserErrors.LockedOut);
        }

        if (!signIn.Succeeded)
        {
            return Result.Failure<AuthTokens>(UserErrors.InvalidCredentials);
        }

        (string accessToken, DateTime accessTokenExpiresAtUtc) = tokenProvider.CreateAccessToken(user);
        (string rawToken, RefreshToken refreshToken) = CreateRefreshToken(user.Id, Guid.CreateVersion7(), dateTimeProvider.UtcNow);

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthTokens(accessToken, accessTokenExpiresAtUtc, rawToken, refreshToken.ExpiresAtUtc));
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        string hash = Hash(refreshToken);

        Guid? familyId = await dbContext.RefreshTokens
            .Where(t => t.TokenHash == hash)
            .Select(t => (Guid?)t.FamilyId)
            .SingleOrDefaultAsync(cancellationToken);

        if (familyId is not null)
        {
            await RevokeFamilyAsync(familyId.Value, dateTimeProvider.UtcNow, cancellationToken);
        }

        // Logout is idempotent: an unknown or already revoked token still ends the session
        return Result.Success();
    }

    public async Task<Result<AuthTokens>> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        DateTime nowUtc = dateTimeProvider.UtcNow;
        string hash = Hash(refreshToken);

        RefreshToken? current = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (current is null)
        {
            return Result.Failure<AuthTokens>(UserErrors.InvalidRefreshToken);
        }

        if (current.RevokedAtUtc is not null)
        {
            // An old token was presented again: theft or a client bug, so end the whole family
            int revokedCount = await RevokeFamilyAsync(current.FamilyId, nowUtc, cancellationToken);
            if (revokedCount > 0)
            {
                logger.LogWarning("Refresh token reuse detected for {UserId} in family {FamilyId}", current.UserId, current.FamilyId);
            }

            return Result.Failure<AuthTokens>(UserErrors.InvalidRefreshToken);
        }

        if (current.ExpiresAtUtc <= nowUtc)
        {
            return Result.Failure<AuthTokens>(UserErrors.InvalidRefreshToken);
        }

        User? user = await userManager.FindByIdAsync(current.UserId.ToString());
        if (user is null || await userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<AuthTokens>(UserErrors.InvalidRefreshToken);
        }

        (string rawToken, RefreshToken next) = CreateRefreshToken(user.Id, current.FamilyId, nowUtc);

        current.RevokedAtUtc = nowUtc;
        current.ReplacedByTokenId = next.Id;
        dbContext.RefreshTokens.Add(next);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Another request rotated the same token at the same moment
            logger.LogWarning(ex, "Refresh token {TokenId} was rotated concurrently", current.Id);
            return Result.Failure<AuthTokens>(UserErrors.InvalidRefreshToken);
        }

        (string accessToken, DateTime accessTokenExpiresAtUtc) = tokenProvider.CreateAccessToken(user);

        return Result.Success(new AuthTokens(accessToken, accessTokenExpiresAtUtc, rawToken, next.ExpiresAtUtc));
    }

    private static Error MapCreateUserErrors(IEnumerable<IdentityError> identityErrors)
    {
        IdentityError[] errors = [.. identityErrors];

        if (errors.Any(e => e.Code is nameof(IdentityErrorDescriber.DuplicateEmail)
                                   or nameof(IdentityErrorDescriber.DuplicateUserName)))
        {
            return UserErrors.EmailTaken;
        }

        return new ValidationError([.. errors.Select(e => Error.Problem($"Users.{e.Code}", e.Description))]);
    }

    private Task<int> RevokeFamilyAsync(Guid familyId, DateTime nowUtc, CancellationToken cancellationToken) =>
        dbContext.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAtUtc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAtUtc, nowUtc), cancellationToken);

    private (string RawToken, RefreshToken Token) CreateRefreshToken(Guid userId, Guid familyId, DateTime nowUtc)
    {
        string rawToken = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));

        var token = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            FamilyId = familyId,
            TokenHash = Hash(rawToken),
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc.AddDays(jwtOptions.Value.RefreshTokenLifetimeDays)
        };

        return (rawToken, token);
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
