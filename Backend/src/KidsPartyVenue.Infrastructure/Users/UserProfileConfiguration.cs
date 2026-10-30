using KidsPartyVenue.Domain.Localization;
using KidsPartyVenue.Domain.Users;
using KidsPartyVenue.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsPartyVenue.Infrastructure.Users;

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.FullName).HasMaxLength(UserProfile.FullNameMaxLength);
        builder.Property(p => p.PreferredLanguage)
            .HasConversion(language => language.ToCode(), code => LanguageCodes.FromCode(code))
            .HasMaxLength(2);

        // Same ID as the Identity user; Domain never references Identity, so the link is configured only here
        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<UserProfile>(p => p.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
