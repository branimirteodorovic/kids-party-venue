using Microsoft.AspNetCore.Identity;

namespace KidsPartyVenue.Infrastructure.Identity;

public sealed class Role : IdentityRole<Guid>
{
    public const string Parent = "Parent";
    public const string ProviderMember = "ProviderMember";
    public const string Admin = "Admin";
}
