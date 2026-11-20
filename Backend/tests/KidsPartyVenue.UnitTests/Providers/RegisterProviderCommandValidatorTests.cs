using AwesomeAssertions;
using FluentValidation.Results;
using KidsPartyVenue.Application.Providers.RegisterProvider;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.UnitTests.Providers;

public sealed class RegisterProviderCommandValidatorTests
{
    private readonly RegisterProviderCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        // Act
        ValidationResult result = _validator.Validate(ValidCommand());

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenOptionalFieldsAreMissing()
    {
        // Arrange
        RegisterProviderCommand command = ValidCommand() with { Description = null, DistrictId = null, Phone = null };

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenNameIsEmpty(string name)
    {
        ShouldFailOn(ValidCommand() with { Name = name }, nameof(RegisterProviderCommand.Name));
    }

    [Fact]
    public void Validate_ShouldFail_WhenNameIsTooLong()
    {
        ShouldFailOn(ValidCommand() with { Name = new string('a', Provider.NameMaxLength + 1) }, nameof(RegisterProviderCommand.Name));
    }

    [Theory]
    [InlineData(ProviderCategory.Catering)]
    [InlineData(ProviderCategory.Cakes)]
    public void Validate_ShouldFail_WhenCategoryIsNotVenue(ProviderCategory category)
    {
        ShouldFailOn(ValidCommand() with { Category = category }, nameof(RegisterProviderCommand.Category));
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionIsTooLong()
    {
        ShouldFailOn(
            ValidCommand() with { Description = new string('a', Provider.DescriptionMaxLength + 1) },
            nameof(RegisterProviderCommand.Description));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenAddressIsEmpty(string address)
    {
        ShouldFailOn(ValidCommand() with { Address = address }, nameof(RegisterProviderCommand.Address));
    }

    [Fact]
    public void Validate_ShouldFail_WhenAddressIsTooLong()
    {
        ShouldFailOn(ValidCommand() with { Address = new string('a', Provider.AddressMaxLength + 1) }, nameof(RegisterProviderCommand.Address));
    }

    [Fact]
    public void Validate_ShouldFail_WhenCityIdIsEmpty()
    {
        ShouldFailOn(ValidCommand() with { CityId = Guid.Empty }, nameof(RegisterProviderCommand.CityId));
    }

    [Fact]
    public void Validate_ShouldFail_WhenDistrictIdIsEmpty()
    {
        ShouldFailOn(ValidCommand() with { DistrictId = Guid.Empty }, nameof(RegisterProviderCommand.DistrictId));
    }

    [Theory]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    public void Validate_ShouldFail_WhenLatitudeIsOutOfRange(double latitude)
    {
        ShouldFailOn(ValidCommand() with { Latitude = latitude }, nameof(RegisterProviderCommand.Latitude));
    }

    [Theory]
    [InlineData(-180.1)]
    [InlineData(180.1)]
    public void Validate_ShouldFail_WhenLongitudeIsOutOfRange(double longitude)
    {
        ShouldFailOn(ValidCommand() with { Longitude = longitude }, nameof(RegisterProviderCommand.Longitude));
    }

    private void ShouldFailOn(RegisterProviderCommand command, string propertyName)
    {
        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.PropertyName.Should().Be(propertyName);
    }

    private static RegisterProviderCommand ValidCommand() =>
        new(
            "Kockica",
            ProviderCategory.Venue,
            "Igraonica za rođendane",
            "Bulevar oslobođenja 10",
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            45.2389,
            19.8441,
            "064 123 4567");
}
