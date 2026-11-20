using AwesomeAssertions;
using FluentValidation.Results;
using KidsPartyVenue.Application.Providers.UpdateBookingPolicy;
using KidsPartyVenue.Domain.BookingPolicies;

namespace KidsPartyVenue.UnitTests.Providers;

public sealed class UpdateBookingPolicyCommandValidatorTests
{
    private readonly UpdateBookingPolicyCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenProviderIdIsSet()
    {
        // Act
        ValidationResult result = _validator.Validate(new UpdateBookingPolicyCommand(Guid.CreateVersion7(), DepositType.None, null, null, 7, 1));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenProviderIdIsEmpty()
    {
        // Act
        ValidationResult result = _validator.Validate(new UpdateBookingPolicyCommand(Guid.Empty, DepositType.None, null, null, 7, 1));

        // Assert
        result.Errors.Should().ContainSingle().Which.PropertyName.Should().Be(nameof(UpdateBookingPolicyCommand.ProviderId));
    }
}
