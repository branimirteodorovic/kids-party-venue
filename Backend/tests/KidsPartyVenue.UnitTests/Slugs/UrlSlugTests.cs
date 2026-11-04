using AwesomeAssertions;
using KidsPartyVenue.Domain.Slugs;

namespace KidsPartyVenue.UnitTests.Slugs;

public sealed class UrlSlugTests
{
    [Theory]
    [InlineData("Rođendanska Žurka", "rodjendanska-zurka")]
    [InlineData("Рођенданска Журка", "rodjendanska-zurka")]
    [InlineData("Igraonica Čarobni Svet", "igraonica-carobni-svet")]
    [InlineData("čćšžđ ČĆŠŽĐ", "ccszdj-ccszdj")]
    [InlineData("Љубав Његош Џак Ђак Ћуп Чаша Шума Жаба", "ljubav-njegos-dzak-djak-cup-casa-suma-zaba")]
    [InlineData("ЉУБАВ ЊЕГОШ ЏАК ЂАК ЋУП", "ljubav-njegos-dzak-djak-cup")]
    [InlineData("Mali Iđoš", "mali-idjos")]
    public void FromName_ShouldWriteSerbianLettersInAscii(string name, string expected)
    {
        // Act
        string slug = UrlSlug.FromName(name);

        // Assert
        slug.Should().Be(expected);
    }

    [Theory]
    [InlineData("Йошкин Ярмарка Юла Ёлка", "joskin-jarmarka-jula-jolka")]
    [InlineData("Мышь Этаж Щука Объём", "mis-etaz-scuka-objom")]
    public void FromName_ShouldMapRussianLetters_ThatSerbianDoesNotHave(string name, string expected)
    {
        // Act
        string slug = UrlSlug.FromName(name);

        // Assert
        slug.Should().Be(expected);
    }

    [Theory]
    [InlineData("Szőke Ünnep Café", "szoke-unnep-cafe")]
    [InlineData("\u01C6ep \u01C9ubav \u01CCiva", "dzep-ljubav-njiva")]
    [InlineData("\uFF2B\uFF49\uFF44\uFF53", "kids")]
    public void FromName_ShouldStripOtherDiacritics_AfterCompatibilityDecomposition(string name, string expected)
    {
        // Act
        string slug = UrlSlug.FromName(name);

        // Assert
        slug.Should().Be(expected);
    }

    [Theory]
    [InlineData("Mike's Party", "mikes-party")]
    [InlineData("Mike\u2019s", "mikes")]
    [InlineData("  Kids & Fun!! ", "kids-fun")]
    [InlineData("Igraonica 123", "igraonica-123")]
    [InlineData("a--b__c...d", "a-b-c-d")]
    [InlineData("-Zeka-", "zeka")]
    [InlineData("2024", "2024")]
    public void FromName_ShouldJoinWordsWithSingleHyphens(string name, string expected)
    {
        // Act
        string slug = UrlSlug.FromName(name);

        // Assert
        slug.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("★★★")]
    [InlineData("'\u2019")]
    public void FromName_ShouldReturnFallback_WhenNothingIsLeft(string? name)
    {
        // Act
        string slug = UrlSlug.FromName(name);

        // Assert
        slug.Should().Be(UrlSlug.Fallback);
    }

    [Fact]
    public void FromName_ShouldCutAtLastHyphen_WhenSlugIsTooLong()
    {
        // Arrange
        string name = $"{new string('a', 55)} {new string('b', 10)}";

        // Act
        string slug = UrlSlug.FromName(name);

        // Assert
        slug.Should().Be(new string('a', 55));
    }

    [Fact]
    public void FromName_ShouldKeepWholeWords_WhenHyphenFallsRightAfterTheLimit()
    {
        // Arrange
        string name = $"{new string('a', UrlSlug.GeneratedMaxLength)} b";

        // Act
        string slug = UrlSlug.FromName(name);

        // Assert
        slug.Should().Be(new string('a', UrlSlug.GeneratedMaxLength));
    }

    [Fact]
    public void FromName_ShouldCutInsideWord_WhenOneWordIsTooLong()
    {
        // Arrange
        string name = new('a', 70);

        // Act
        string slug = UrlSlug.FromName(name);

        // Assert
        slug.Should().Be(new string('a', UrlSlug.GeneratedMaxLength));
    }

    [Theory]
    [InlineData("novi-sad")]
    [InlineData("a")]
    [InlineData("123")]
    [InlineData("kockica-2")]
    public void IsValid_ShouldAccept_WhenSlugIsWellFormed(string slug)
    {
        // Act
        bool valid = UrlSlug.IsValid(slug);

        // Assert
        valid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Novi-Sad")]
    [InlineData("novi sad")]
    [InlineData("novi--sad")]
    [InlineData("-novi")]
    [InlineData("novi-")]
    [InlineData("čigra")]
    public void IsValid_ShouldReject_WhenSlugIsMalformed(string? slug)
    {
        // Act
        bool valid = UrlSlug.IsValid(slug);

        // Assert
        valid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReject_WhenSlugIsLongerThanMaxLength()
    {
        // Act
        bool atLimit = UrlSlug.IsValid(new string('a', UrlSlug.MaxLength));
        bool overLimit = UrlSlug.IsValid(new string('a', UrlSlug.MaxLength + 1));

        // Assert
        atLimit.Should().BeTrue();
        overLimit.Should().BeFalse();
    }
}
