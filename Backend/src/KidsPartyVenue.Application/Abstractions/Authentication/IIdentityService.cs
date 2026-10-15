using KidsPartyVenue.Domain;

namespace KidsPartyVenue.Application.Abstractions.Authentication;

public interface IIdentityService
{
    Task<Result<Guid>> RegisterAsync(string email, string password, string fullName, CancellationToken cancellationToken);
    Task<Result<AuthTokens>> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result<AuthTokens>> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}
