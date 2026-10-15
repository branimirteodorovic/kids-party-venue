using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Users.Register;

public sealed record RegisterUserCommand(string Email, string Password, string FullName) : ICommand<Guid>;
