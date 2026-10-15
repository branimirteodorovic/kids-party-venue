using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;

namespace KidsPartyVenue.Application.Users.Logout;

internal sealed class LogoutCommandHandler(IIdentityService identityService) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        return await identityService.LogoutAsync(command.RefreshToken, cancellationToken);
    }
}
