using FluentValidation;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.CreateProvider;

internal sealed class CreateProviderCommandValidator : AbstractValidator<CreateProviderCommand>
{
    public CreateProviderCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(Provider.NameMaxLength);

        // Other categories open with P2.11
        RuleFor(c => c.Category)
            .Equal(ProviderCategory.Venue)
            .WithMessage("Only venues can be created");

        RuleFor(c => c.Description)
            .MaximumLength(Provider.DescriptionMaxLength);

        RuleFor(c => c.Address)
            .NotEmpty()
            .MaximumLength(Provider.AddressMaxLength);

        RuleFor(c => c.CityId)
            .NotEmpty();

        RuleFor(c => c.DistrictId)
            .NotEqual(Guid.Empty);

        RuleFor(c => c.Latitude)
            .InclusiveBetween(-90, 90);

        RuleFor(c => c.Longitude)
            .InclusiveBetween(-180, 180);
    }
}
