using AwesomeAssertions;
using FluentValidation.Results;
using KidsPartyVenue.Application.Providers.UpdateProviderBasicInfo;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.UnitTests.Providers;

public sealed class UpdateProviderBasicInfoCommandValidatorTests
{
    private readonly UpdateProviderBasicInfoCommandValidator _validator = new();

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
        // Act
        ValidationResult result = _validator.Validate(ValidCommand() with { Description = null, DistrictId = null, Phone = null });

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenProviderIdIsEmpty()
    {
        ShouldFailOn(ValidCommand() with { ProviderId = Guid.Empty }, nameof(UpdateProviderBasicInfoCommand.ProviderId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenNameIsEmpty(string name)
    {
        ShouldFailOn(ValidCommand() with { Name = name }, nameof(UpdateProviderBasicInfoCommand.Name));
    }

    [Fact]
    public void Validate_ShouldFail_WhenNameIsTooLong()
    {
        ShouldFailOn(
            ValidCommand() with { Name = new string('a', Provider.NameMaxLength + 1) },
            nameof(UpdateProviderBasicInfoCommand.Name));
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionIsTooLong()
    {
        ShouldFailOn(
            ValidCommand() with { Description = new string('a', Provider.DescriptionMaxLength + 1) },
            nameof(UpdateProviderBasicInfoCommand.Description));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenAddressIsEmpty(string address)
    {
        ShouldFailOn(ValidCommand() with { Address = address }, nameof(UpdateProviderBasicInfoCommand.Address));
    }

    [Fact]
    public void Validate_ShouldFail_WhenAddressIsTooLong()
    {
        ShouldFailOn(
            ValidCommand() with { Address = new string('a', Provider.AddressMaxLength + 1) },
            nameof(UpdateProviderBasicInfoCommand.Address));
    }

    [Fact]
    public void Validate_ShouldFail_WhenCityIdIsEmpty()
    {
        ShouldFailOn(ValidCommand() with { CityId = Guid.Empty }, nameof(UpdateProviderBasicInfoCommand.CityId));
    }

    [Fact]
    public void Validate_ShouldFail_WhenDistrictIdIsEmpty()
    {
        ShouldFailOn(ValidCommand() with { DistrictId = Guid.Empty }, nameof(UpdateProviderBasicInfoCommand.DistrictId));
    }

    [Theory]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    public void Validate_ShouldFail_WhenLatitudeIsOutOfRange(double latitude)
    {
        ShouldFailOn(ValidCommand() with { Latitude = latitude }, nameof(UpdateProviderBasicInfoCommand.Latitude));
    }

    [Theory]
    [InlineData(-180.1)]
    [InlineData(180.1)]
    public void Validate_ShouldFail_WhenLongitudeIsOutOfRange(double longitude)
    {
        ShouldFailOn(ValidCommand() with { Longitude = longitude }, nameof(UpdateProviderBasicInfoCommand.Longitude));
    }

    private void ShouldFailOn(UpdateProviderBasicInfoCommand command, string propertyName)
    {
        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.PropertyName.Should().Be(propertyName);
    }

    private static UpdateProviderBasicInfoCommand ValidCommand() =>
        new(
            Guid.CreateVersion7(),
            "Kockica",
            "Igraonica za rođendane",
            "Bulevar oslobođenja 10",
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            45.2389,
            19.8441,
            "064 123 4567",
            Version: 1234);
}
