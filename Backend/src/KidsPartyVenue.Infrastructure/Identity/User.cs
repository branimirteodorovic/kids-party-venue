using Microsoft.AspNetCore.Identity;

namespace KidsPartyVenue.Infrastructure.Identity;

public sealed class User : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
