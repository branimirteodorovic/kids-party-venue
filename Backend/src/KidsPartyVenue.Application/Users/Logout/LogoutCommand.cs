using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Users.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand;
