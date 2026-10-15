using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;

namespace KidsPartyVenue.Application.Users.Register;

internal sealed class RegisterUserCommandHandler(IIdentityService identityService) : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        return await identityService.RegisterAsync(command.Email, command.Password, command.FullName, cancellationToken);
    }
}
