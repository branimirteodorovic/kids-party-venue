using AwesomeAssertions;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Slugs;

namespace KidsPartyVenue.UnitTests.Providers;

public sealed class ProviderSlugTests
{
    [Fact]
    public void MakeUnique_ShouldReturnBase_WhenItIsFree()
    {
        // Act
        string slug = ProviderSlug.MakeUnique("kockica", ["kockica-bar"]);

        // Assert
        slug.Should().Be("kockica");
    }

    [Fact]
    public void MakeUnique_ShouldAddTwo_WhenBaseIsTaken()
    {
        // Act
        string slug = ProviderSlug.MakeUnique("kockica", ["kockica"]);

        // Assert
        slug.Should().Be("kockica-2");
    }

    [Fact]
    public void MakeUnique_ShouldAddNextNumber_WhenBaseAndTwoAreTaken()
    {
        // Act
        string slug = ProviderSlug.MakeUnique("kockica", ["kockica", "kockica-2"]);

        // Assert
        slug.Should().Be("kockica-3");
    }

    [Fact]
    public void MakeUnique_ShouldAddLowestFreeNumber_WhenThereIsAGap()
    {
        // Act
        string slug = ProviderSlug.MakeUnique("kockica", ["kockica", "kockica-3"]);

        // Assert
        slug.Should().Be("kockica-2");
    }

    [Fact]
    public void MakeUnique_ShouldAddNumber_WhenBaseIsReserved()
    {
        // Act
        string slug = ProviderSlug.MakeUnique("kvart", []);

        // Assert
        slug.Should().Be("kvart-2");
    }

    [Fact]
    public void MakeUnique_ShouldNotShortenBase_WhenItHasGeneratedMaxLength()
    {
        // Arrange
        string baseSlug = new('a', UrlSlug.GeneratedMaxLength);

        // Act
        string slug = ProviderSlug.MakeUnique(baseSlug, [baseSlug]);

        // Assert
        slug.Should().Be($"{baseSlug}-2");
        UrlSlug.IsValid(slug).Should().BeTrue();
    }

    [Theory]
    [InlineData("kvart", true)]
    [InlineData("kvartovi", false)]
    [InlineData("kvart-2", false)]
    public void IsReserved_ShouldMatchReservedWordsOnly(string slug, bool expected)
    {
        // Act
        bool reserved = ProviderSlug.IsReserved(slug);

        // Assert
        reserved.Should().Be(expected);
    }
}
