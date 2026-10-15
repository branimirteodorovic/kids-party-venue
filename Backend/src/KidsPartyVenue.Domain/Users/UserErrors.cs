namespace KidsPartyVenue.Domain.Users;

public static class UserErrors
{
    public static readonly Error InvalidCredentials = Error.Unauthorized("Users.InvalidCredentials", "Email or password is incorrect");
    public static readonly Error EmailTaken = Error.Conflict("Users.EmailTaken", "Email is taken");
    public static readonly Error LockedOut = Error.Forbidden("Users.LockedOut", "Account is locked out");
    public static readonly Error RoleAssignmentFailed = Error.Failure("Users.RoleAssignmentFailed", "Could not assign the default role to the user");
    public static readonly Error InvalidRefreshToken = Error.Unauthorized("Users.InvalidRefreshToken", "Refresh token is invalid");
    public static readonly Error NotFound = Error.NotFound("Users.NotFound", "User is not found");
}
