using AwesomeAssertions;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Phones;

namespace KidsPartyVenue.UnitTests.Phones;

public sealed class PhoneNumberTests
{
    [Theory]
    [InlineData("064/123-4567")]
    [InlineData("064 123 4567")]
    [InlineData("(064) 123.45.67")]
    [InlineData("+381 64 123 4567")]
    [InlineData("00381641234567")]
    public void Create_ShouldReturnE164_WhenNumberIsSerbian(string number)
    {
        // Act
        Result<PhoneNumber> result = PhoneNumber.Create(number);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("+381641234567");
    }

    [Theory]
    [InlineData("021 456 789", "+38121456789")]
    [InlineData("+43 1 234 5678", "+4312345678")]
    [InlineData("0043 1 234 5678", "+4312345678")]
    public void Create_ShouldKeepTheCountryCode_WhenNumberHasOne(string number, string expected)
    {
        // Act
        Result<PhoneNumber> result = PhoneNumber.Create(number);

        // Assert
        result.Value.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("064 abc 4567")]
    [InlineData("0641")]
    [InlineData("+381 64 123 4567 8901")]
    [InlineData("+0641234567")]
    [InlineData("000641234567")]
    [InlineData("+381 64 123 4567 ext 2")]
    public void Create_ShouldFail_WhenNumberIsInvalid(string? number)
    {
        // Act
        Result<PhoneNumber> result = PhoneNumber.Create(number);

        // Assert
        result.Error.Should().Be(PhoneErrors.InvalidNumber);
    }
}
