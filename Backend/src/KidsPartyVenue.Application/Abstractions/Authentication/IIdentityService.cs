using KidsPartyVenue.Domain;

namespace KidsPartyVenue.Application.Abstractions.Authentication;

public interface IIdentityService
{
    /// <summary>Creates the login account with the Parent role. Profile data lives in <c>UserProfile</c>.</summary>
    Task<Result> RegisterAsync(Guid userId, string email, string password, CancellationToken cancellationToken);

    /// <summary>
    /// Gives the user the ProviderMember role, keeping their other roles. A user who has it already is left as is. It
    /// saves at once, so the caller runs it inside its transaction.
    /// </summary>
    Task<Result> EnsureProviderMemberRoleAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<AuthTokens>> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result<AuthTokens>> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}
