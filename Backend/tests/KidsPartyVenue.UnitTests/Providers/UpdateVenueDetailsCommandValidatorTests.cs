using AwesomeAssertions;
using FluentValidation.Results;
using KidsPartyVenue.Application.Providers.UpdateVenueDetails;
using KidsPartyVenue.Domain.Venues;

namespace KidsPartyVenue.UnitTests.Providers;

public sealed class UpdateVenueDetailsCommandValidatorTests
{
    private readonly UpdateVenueDetailsCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenProviderIdIsSet()
    {
        // Act
        ValidationResult result = _validator.Validate(Command(Guid.CreateVersion7()));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenProviderIdIsEmpty()
    {
        // Act
        ValidationResult result = _validator.Validate(Command(Guid.Empty));

        // Assert
        result.Errors.Should().ContainSingle().Which.PropertyName.Should().Be(nameof(UpdateVenueDetailsCommand.ProviderId));
    }

    private static UpdateVenueDetailsCommand Command(Guid providerId) =>
        new(providerId, 30, null, 3, 10, OutsideFoodRule.NotAllowed, OutsideFoodRule.Allowed, FoodService.ForPurchase,
            HasOwnAnimator: true, HasOwnPhotographer: false, HasSmokingArea: false, Version: 1);
}
