using FluentValidation;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.UpdateProviderBasicInfo;

internal sealed class UpdateProviderBasicInfoCommandValidator : AbstractValidator<UpdateProviderBasicInfoCommand>
{
    public UpdateProviderBasicInfoCommandValidator()
    {
        RuleFor(c => c.ProviderId)
            .NotEmpty();

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(Provider.NameMaxLength);

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
