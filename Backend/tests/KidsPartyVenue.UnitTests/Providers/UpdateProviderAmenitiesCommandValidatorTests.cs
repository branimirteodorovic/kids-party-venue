using AwesomeAssertions;
using FluentValidation.Results;
using KidsPartyVenue.Application.Providers.UpdateProviderAmenities;

namespace KidsPartyVenue.UnitTests.Providers;

public sealed class UpdateProviderAmenitiesCommandValidatorTests
{
    private readonly UpdateProviderAmenitiesCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        // Act
        ValidationResult result = _validator.Validate(new UpdateProviderAmenitiesCommand(Guid.CreateVersion7(), [Guid.CreateVersion7()], 1));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenListIsEmpty()
    {
        // Act
        ValidationResult result = _validator.Validate(new UpdateProviderAmenitiesCommand(Guid.CreateVersion7(), [], 1));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenProviderIdIsEmpty()
    {
        // Act
        ValidationResult result = _validator.Validate(new UpdateProviderAmenitiesCommand(Guid.Empty, [], 1));

        // Assert
        result.Errors.Should().ContainSingle().Which.PropertyName.Should().Be(nameof(UpdateProviderAmenitiesCommand.ProviderId));
    }

    [Fact]
    public void Validate_ShouldFail_WhenListIsNull()
    {
        // Act
        ValidationResult result = _validator.Validate(new UpdateProviderAmenitiesCommand(Guid.CreateVersion7(), null!, 1));

        // Assert
        result.Errors.Should().ContainSingle().Which.PropertyName.Should().Be(nameof(UpdateProviderAmenitiesCommand.AmenityIds));
    }

    [Fact]
    public void Validate_ShouldFail_WhenAnAmenityIdIsEmpty()
    {
        // Act
        ValidationResult result = _validator.Validate(
            new UpdateProviderAmenitiesCommand(Guid.CreateVersion7(), [Guid.CreateVersion7(), Guid.Empty], 1));

        // Assert
        result.Errors.Should().ContainSingle().Which.PropertyName.Should().Be("AmenityIds[1]");
    }
}
