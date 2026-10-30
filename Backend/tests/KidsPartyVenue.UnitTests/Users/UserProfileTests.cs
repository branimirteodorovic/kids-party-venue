using AwesomeAssertions;
using Bogus;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Localization;
using KidsPartyVenue.Domain.Users;

namespace KidsPartyVenue.UnitTests.Users;

public sealed class UserProfileTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void Create_ShouldReturnProfile_WhenInputIsValid()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        string fullName = Faker.Name.FullName();
        DateTime createdAtUtc = DateTime.UtcNow;

        // Act
        Result<UserProfile> result = UserProfile.Create(userId, $"  {fullName} ", Language.Ru, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(userId);
        result.Value.FullName.Should().Be(fullName);
        result.Value.PreferredLanguage.Should().Be(Language.Ru);
        result.Value.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_ShouldFail_WhenFullNameIsBlank(string? fullName)
    {
        // Act
        Result<UserProfile> result = UserProfile.Create(Guid.CreateVersion7(), fullName!, Language.Sr, DateTime.UtcNow);

        // Assert
        result.Error.Should().Be(UserErrors.FullNameRequired);
    }

    [Fact]
    public void Create_ShouldFail_WhenFullNameIsTooLong()
    {
        // Arrange
        string fullName = new('a', UserProfile.FullNameMaxLength + 1);

        // Act
        Result<UserProfile> result = UserProfile.Create(Guid.CreateVersion7(), fullName, Language.Sr, DateTime.UtcNow);

        // Assert
        result.Error.Should().Be(UserErrors.FullNameTooLong);
    }

    [Fact]
    public void Create_ShouldFail_WhenLanguageIsUndefined()
    {
        // Act
        Result<UserProfile> result = UserProfile.Create(Guid.CreateVersion7(), Faker.Name.FullName(), (Language)42, DateTime.UtcNow);

        // Assert
        result.Error.Should().Be(LocalizationErrors.UnsupportedLanguage);
    }

    [Fact]
    public void ChangePreferredLanguage_ShouldUpdateLanguage_WhenLanguageIsSupported()
    {
        // Arrange
        UserProfile profile = UserProfile.Create(Guid.CreateVersion7(), Faker.Name.FullName(), Language.Sr, DateTime.UtcNow).Value;

        // Act
        Result result = profile.ChangePreferredLanguage(Language.En);

        // Assert
        result.IsSuccess.Should().BeTrue();
        profile.PreferredLanguage.Should().Be(Language.En);
    }

    [Fact]
    public void ChangePreferredLanguage_ShouldKeepLanguage_WhenLanguageIsUndefined()
    {
        // Arrange
        UserProfile profile = UserProfile.Create(Guid.CreateVersion7(), Faker.Name.FullName(), Language.Sr, DateTime.UtcNow).Value;

        // Act
        Result result = profile.ChangePreferredLanguage((Language)42);

        // Assert
        result.Error.Should().Be(LocalizationErrors.UnsupportedLanguage);
        profile.PreferredLanguage.Should().Be(Language.Sr);
    }
}
