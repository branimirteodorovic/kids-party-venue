using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;

namespace KidsPartyVenue.Application.Users.Login;

internal sealed class LoginUserCommandHandler(IIdentityService identityService) : ICommandHandler<LoginUserCommand, AuthTokens>
{
    public async Task<Result<AuthTokens>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        return await identityService.LoginAsync(command.Email, command.Password, cancellationToken);
    }
}
