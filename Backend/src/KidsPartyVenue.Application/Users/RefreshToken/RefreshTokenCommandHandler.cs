using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;

namespace KidsPartyVenue.Application.Users.RefreshToken;

internal sealed class RefreshTokenCommandHandler(IIdentityService identityService) : ICommandHandler<RefreshTokenCommand, AuthTokens>
{
    public async Task<Result<AuthTokens>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        return await identityService.RefreshAsync(command.RefreshToken, cancellationToken);
    }
}
