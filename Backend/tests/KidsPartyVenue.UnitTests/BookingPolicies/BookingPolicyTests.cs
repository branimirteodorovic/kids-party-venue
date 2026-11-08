using System.Globalization;
using AwesomeAssertions;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.BookingPolicies;

namespace KidsPartyVenue.UnitTests.BookingPolicies;

public sealed class BookingPolicyTests
{
    [Fact]
    public void Create_ShouldReturnPolicy_WhenThereIsNoDeposit()
    {
        // Act
        Result<BookingPolicy> result = BookingPolicy.Create(DepositType.None, null, null, 7);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DepositType.Should().Be(DepositType.None);
        result.Value.DepositValue.Should().BeNull();
        result.Value.DepositDueDays.Should().BeNull();
        result.Value.FreeCancellationDays.Should().Be(7);
    }

    [Theory]
    [InlineData(DepositType.Percentage, 30)]
    [InlineData(DepositType.FixedAmount, 5000)]
    public void Create_ShouldReturnPolicy_WhenDepositIsValid(DepositType depositType, int depositValue)
    {
        // Act
        Result<BookingPolicy> result = BookingPolicy.Create(depositType, depositValue, 3, 14);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DepositType.Should().Be(depositType);
        result.Value.DepositValue.Should().Be(depositValue);
        result.Value.DepositDueDays.Should().Be(3);
        result.Value.FreeCancellationDays.Should().Be(14);
    }

    [Fact]
    public void Create_ShouldFail_WhenDepositTypeIsUnknown()
    {
        // Act
        Result<BookingPolicy> result = BookingPolicy.Create((DepositType)99, null, null, 7);

        // Assert
        result.Error.Should().Be(BookingPolicyErrors.UnknownDepositType);
    }

    [Theory]
    [InlineData(10, null)]
    [InlineData(null, 3)]
    [InlineData(10, 3)]
    public void Create_ShouldFail_WhenDepositTermsAreGivenWithoutDeposit(int? depositValue, int? depositDueDays)
    {
        // Act
        Result<BookingPolicy> result = BookingPolicy.Create(DepositType.None, depositValue, depositDueDays, 7);

        // Assert
        result.Error.Should().Be(BookingPolicyErrors.DepositTermsWithoutDeposit);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("100")]
    public void Create_ShouldAcceptPercentage_WhenItIsAtTheLimits(string percentage)
    {
        // Act
        Result<BookingPolicy> result = BookingPolicy.Create(DepositType.Percentage, ParseOrNull(percentage), 3, 7);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("101")]
    [InlineData("12.5")]
    [InlineData("-10")]
    public void Create_ShouldFail_WhenPercentageIsInvalid(string? percentage)
    {
        // Act
        Result<BookingPolicy> result = BookingPolicy.Create(DepositType.Percentage, ParseOrNull(percentage), 3, 7);

        // Assert
        result.Error.Should().Be(BookingPolicyErrors.InvalidDepositPercentage);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("100000")]
    public void Create_ShouldAcceptAmount_WhenItIsAtTheLimits(string amount)
    {
        // Act
        Result<BookingPolicy> result = BookingPolicy.Create(DepositType.FixedAmount, ParseOrNull(amount), 3, 7);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("100001")]
    [InlineData("100.50")]
    public void Create_ShouldFail_WhenAmountIsInvalid(string? amount)
    {
        // Act
        Result<BookingPolicy> result = BookingPolicy.Create(DepositType.FixedAmount, ParseOrNull(amount), 3, 7);

        // Assert
        result.Error.Should().Be(BookingPolicyErrors.InvalidDepositAmount);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    public void Create_ShouldAcceptDueDays_WhenTheyAreAtTheLimits(int dueDays)
    {
        // Act
        Result<BookingPolicy> result = BookingPolicy.Create(DepositType.Percentage, 30, dueDays, 7);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(31)]
    public void Create_ShouldFail_WhenDueDaysAreInvalid(int? dueDays)
    {
        // Act
        Result<BookingPolicy> result = BookingPolicy.Create(DepositType.Percentage, 30, dueDays, 7);

        // Assert
        result.Error.Should().Be(BookingPolicyErrors.InvalidDepositDueDays);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(180)]
    public void Create_ShouldAcceptFreeCancellationDays_WhenTheyAreAtTheLimits(int days)
    {
        // Act
        Result<BookingPolicy> result = BookingPolicy.Create(DepositType.None, null, null, days);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(181)]
    public void Create_ShouldFail_WhenFreeCancellationDaysAreInvalid(int days)
    {
        // Act
        Result<BookingPolicy> result = BookingPolicy.Create(DepositType.None, null, null, days);

        // Assert
        result.Error.Should().Be(BookingPolicyErrors.InvalidFreeCancellationDays);
    }

    [Fact]
    public void Policies_ShouldBeEqual_WhenTheirValuesAreEqual()
    {
        // Arrange
        BookingPolicy first = BookingPolicy.Create(DepositType.Percentage, 30, 3, 7).Value;
        BookingPolicy second = BookingPolicy.Create(DepositType.Percentage, 30, 3, 7).Value;
        BookingPolicy other = BookingPolicy.Create(DepositType.Percentage, 30, 3, 8).Value;

        // Assert
        first.Should().Be(second);
        first.Should().NotBe(other);
    }

    private static decimal? ParseOrNull(string? value) =>
        value is null ? null : decimal.Parse(value, CultureInfo.InvariantCulture);
}
