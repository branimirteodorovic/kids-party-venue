using Microsoft.AspNetCore.Identity;

namespace KidsPartyVenue.Infrastructure.Identity;

public sealed class User : IdentityUser<Guid>
{
    public DateTime CreatedAtUtc { get; set; }
}
