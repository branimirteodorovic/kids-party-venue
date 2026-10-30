using KidsPartyVenue.Domain;

namespace KidsPartyVenue.Application.Abstractions.Authentication;

public interface IIdentityService
{
    /// <summary>Creates the login account with the Parent role. Profile data lives in <c>UserProfile</c>.</summary>
    Task<Result> RegisterAsync(Guid userId, string email, string password, CancellationToken cancellationToken);
    Task<Result<AuthTokens>> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result<AuthTokens>> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}
