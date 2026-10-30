using AwesomeAssertions;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Localization;

namespace KidsPartyVenue.UnitTests.Localization;

public sealed class LocalizedTextTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenSerbianIsBlank(string sr)
    {
        // Act
        Result<LocalizedText> result = LocalizedText.Create(sr, "Name", "Имя");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LocalizationErrors.SerbianTextRequired);
    }

    [Fact]
    public void Create_ShouldTrimAllLanguages_WhenTextsHaveSurroundingSpaces()
    {
        // Act
        LocalizedText text = LocalizedText.Create(" Igraonica ", " Playroom ", " Игровая ").Value;

        // Assert
        text.Sr.Should().Be("Igraonica");
        text.En.Should().Be("Playroom");
        text.Ru.Should().Be("Игровая");
    }

    [Theory]
    [InlineData(Language.Sr, "Igraonica")]
    [InlineData(Language.En, "Playroom")]
    [InlineData(Language.Ru, "Игровая")]
    public void Get_ShouldReturnTranslation_WhenItExists(Language language, string expected)
    {
        // Arrange
        LocalizedText text = LocalizedText.Create("Igraonica", "Playroom", "Игровая").Value;

        // Act
        string value = text.Get(language);

        // Assert
        value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("  ", "  ")]
    public void Get_ShouldFallBackToSerbian_WhenTranslationIsMissingOrBlank(string? en, string? ru)
    {
        // Arrange
        LocalizedText text = LocalizedText.Create("Igraonica", en, ru).Value;

        // Act & Assert
        text.Get(Language.En).Should().Be("Igraonica");
        text.Get(Language.Ru).Should().Be("Igraonica");
        text.En.Should().BeNull();
        text.Ru.Should().BeNull();
    }
}
