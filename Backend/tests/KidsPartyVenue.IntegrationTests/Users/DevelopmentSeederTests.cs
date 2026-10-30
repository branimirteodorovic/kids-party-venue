using AwesomeAssertions;
using KidsPartyVenue.Domain.Localization;
using KidsPartyVenue.Domain.Users;
using KidsPartyVenue.Infrastructure.Identity;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.IntegrationTests.Users;

public sealed class DevelopmentSeederTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private readonly IntegrationTestWebAppFactory _factory = factory;

    [Fact]
    public async Task SeedDevelopmentData_ShouldCreateAdminProfile_WhenAdminIsSeeded()
    {
        // Arrange
        string adminEmail = _factory.Services.GetRequiredService<IConfiguration>()["Seed:AdminEmail"]!;

        // Act: the seeder ran when the test host started
        User admin = await DbContext.Users.AsNoTracking().SingleAsync(u => u.Email == adminEmail);
        UserProfile? profile = await DbContext.UserProfiles.AsNoTracking().SingleOrDefaultAsync(p => p.Id == admin.Id);

        // Assert
        profile.Should().NotBeNull();
        profile!.FullName.Should().Be("Administrator");
        profile.PreferredLanguage.Should().Be(LanguageCodes.Default);
    }
}
