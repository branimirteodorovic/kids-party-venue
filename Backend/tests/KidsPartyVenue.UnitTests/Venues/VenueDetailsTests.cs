using AwesomeAssertions;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Venues;

namespace KidsPartyVenue.UnitTests.Venues;

public sealed class VenueDetailsTests
{
    [Fact]
    public void Create_ShouldReturnDetails_WhenInputIsValid()
    {
        // Act
        Result<VenueDetails> result = VenueDetails.Create(
            30, 20, 3, 10, OutsideFoodRule.NotAllowed, OutsideFoodRule.AllowedWithFee, FoodService.Included,
            hasOwnAnimator: true, hasOwnPhotographer: false, hasSmokingArea: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MaxChildren.Should().Be(30);
        result.Value.MaxAdults.Should().Be(20);
        result.Value.MinAge.Should().Be(3);
        result.Value.MaxAge.Should().Be(10);
        result.Value.OutsideCatering.Should().Be(OutsideFoodRule.NotAllowed);
        result.Value.OutsideCake.Should().Be(OutsideFoodRule.AllowedWithFee);
        result.Value.FoodService.Should().Be(FoodService.Included);
        result.Value.HasOwnAnimator.Should().BeTrue();
        result.Value.HasOwnPhotographer.Should().BeFalse();
        result.Value.HasSmokingArea.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnDetails_WhenMaxAdultsIsMissing()
    {
        // Act
        Result<VenueDetails> result = Create(maxAdults: null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MaxAdults.Should().BeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(500)]
    public void Create_ShouldAcceptMaxChildren_AtItsLimits(int maxChildren)
    {
        // Act
        Result<VenueDetails> result = Create(maxChildren: maxChildren);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(501)]
    public void Create_ShouldFail_WhenMaxChildrenIsOutOfRange(int maxChildren)
    {
        // Act
        Result<VenueDetails> result = Create(maxChildren: maxChildren);

        // Assert
        result.Error.Should().Be(VenueDetailsErrors.InvalidMaxChildren);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(500)]
    public void Create_ShouldAcceptMaxAdults_AtItsLimits(int maxAdults)
    {
        // Act
        Result<VenueDetails> result = Create(maxAdults: maxAdults);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(501)]
    public void Create_ShouldFail_WhenMaxAdultsIsOutOfRange(int maxAdults)
    {
        // Act
        Result<VenueDetails> result = Create(maxAdults: maxAdults);

        // Assert
        result.Error.Should().Be(VenueDetailsErrors.InvalidMaxAdults);
    }

    [Theory]
    [InlineData(0, 18)]
    [InlineData(6, 6)]
    public void Create_ShouldAcceptAges_AtTheirLimits(int minAge, int maxAge)
    {
        // Act
        Result<VenueDetails> result = Create(minAge: minAge, maxAge: maxAge);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(19, 19)]
    [InlineData(3, -1)]
    [InlineData(3, 19)]
    public void Create_ShouldFail_WhenAnAgeIsOutOfRange(int minAge, int maxAge)
    {
        // Act
        Result<VenueDetails> result = Create(minAge: minAge, maxAge: maxAge);

        // Assert
        result.Error.Should().Be(VenueDetailsErrors.InvalidAge);
    }

    [Fact]
    public void Create_ShouldFail_WhenMinAgeIsGreaterThanMaxAge()
    {
        // Act
        Result<VenueDetails> result = Create(minAge: 7, maxAge: 6);

        // Assert
        result.Error.Should().Be(VenueDetailsErrors.InvalidAgeRange);
    }

    [Fact]
    public void Create_ShouldFail_WhenOutsideCateringRuleIsUnknown()
    {
        // Act
        Result<VenueDetails> result = Create(outsideCatering: (OutsideFoodRule)99);

        // Assert
        result.Error.Should().Be(VenueDetailsErrors.UnknownOutsideFoodRule);
    }

    [Fact]
    public void Create_ShouldFail_WhenOutsideCakeRuleIsUnknown()
    {
        // Act
        Result<VenueDetails> result = Create(outsideCake: (OutsideFoodRule)99);

        // Assert
        result.Error.Should().Be(VenueDetailsErrors.UnknownOutsideFoodRule);
    }

    [Fact]
    public void Create_ShouldFail_WhenFoodServiceIsUnknown()
    {
        // Act
        Result<VenueDetails> result = Create(foodService: (FoodService)99);

        // Assert
        result.Error.Should().Be(VenueDetailsErrors.UnknownFoodService);
    }

    [Fact]
    public void Details_ShouldBeEqual_WhenValuesAreEqual()
    {
        // Act
        VenueDetails first = Create().Value;
        VenueDetails second = Create().Value;
        VenueDetails other = Create(hasSmokingArea: true).Value;

        // Assert
        first.Should().Be(second);
        other.Should().NotBe(first);
    }

    private static Result<VenueDetails> Create(
        int maxChildren = 30,
        int? maxAdults = 20,
        int minAge = 3,
        int maxAge = 10,
        OutsideFoodRule outsideCatering = OutsideFoodRule.NotAllowed,
        OutsideFoodRule outsideCake = OutsideFoodRule.Allowed,
        FoodService foodService = FoodService.ForPurchase,
        bool hasSmokingArea = false) =>
        VenueDetails.Create(
            maxChildren, maxAdults, minAge, maxAge, outsideCatering, outsideCake, foodService,
            hasOwnAnimator: false, hasOwnPhotographer: false, hasSmokingArea);
}
