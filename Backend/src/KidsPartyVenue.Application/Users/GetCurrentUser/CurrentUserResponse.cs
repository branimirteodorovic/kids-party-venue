namespace KidsPartyVenue.Application.Users.GetCurrentUser;

public sealed record CurrentUserResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = [];
    public List<string> Permissions { get; init; } = [];
}
