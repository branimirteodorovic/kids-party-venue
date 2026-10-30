using AwesomeAssertions;
using KidsPartyVenue.Domain.Localization;

namespace KidsPartyVenue.UnitTests.Localization;

public sealed class LanguageCodesTests
{
    [Theory]
    [InlineData(Language.Sr, "sr")]
    [InlineData(Language.En, "en")]
    [InlineData(Language.Ru, "ru")]
    public void ToCode_ShouldReturnTwoLetterCode_WhenLanguageIsSupported(Language language, string code)
    {
        // Act & Assert
        language.ToCode().Should().Be(code);
        LanguageCodes.FromCode(code).Should().Be(language);
    }

    [Theory]
    [InlineData("SR", Language.Sr)]
    [InlineData("En", Language.En)]
    public void TryFromCode_ShouldIgnoreCase_WhenCodeIsSupported(string code, Language expected)
    {
        // Act
        bool found = LanguageCodes.TryFromCode(code, out Language language);

        // Assert
        found.Should().BeTrue();
        language.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("de")]
    [InlineData("en-US")]
    public void TryFromCode_ShouldReturnDefault_WhenCodeIsNotSupported(string? code)
    {
        // Act
        bool found = LanguageCodes.TryFromCode(code, out Language language);

        // Assert
        found.Should().BeFalse();
        language.Should().Be(LanguageCodes.Default);
    }

    [Fact]
    public void FromCode_ShouldThrow_WhenCodeIsNotSupported()
    {
        // Act
        Action act = () => LanguageCodes.FromCode("de");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void All_ShouldContainEveryLanguage()
    {
        // Act & Assert
        LanguageCodes.All.Should().BeEquivalentTo(Enum.GetValues<Language>());
    }
}
