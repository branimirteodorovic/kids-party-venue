namespace KidsPartyVenue.Domain.Users;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    void Insert(UserProfile profile);
}
