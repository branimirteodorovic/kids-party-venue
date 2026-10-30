using KidsPartyVenue.Domain.Users;
using KidsPartyVenue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.Infrastructure.Users;

internal sealed class UserProfileRepository(KidsPartyVenueDbContext context) : IUserProfileRepository
{
    public Task<UserProfile?> GetByIdAsync(Guid userId, CancellationToken cancellationToken) =>
        context.UserProfiles.SingleOrDefaultAsync(p => p.Id == userId, cancellationToken);

    public void Insert(UserProfile profile) => context.UserProfiles.Add(profile);
}
