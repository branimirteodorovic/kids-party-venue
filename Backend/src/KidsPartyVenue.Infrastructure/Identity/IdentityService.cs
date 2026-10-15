using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Users;
using KidsPartyVenue.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace KidsPartyVenue.Infrastructure.Identity;

internal sealed class IdentityService(
    IDateTimeProvider dateTimeProvider,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    TokenProvider tokenProvider,
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

        // TODO: issue a refresh token
        return Result.Success(new AuthTokens(accessToken, accessTokenExpiresAtUtc, string.Empty, default));
    }

    public Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<AuthTokens>> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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
}
