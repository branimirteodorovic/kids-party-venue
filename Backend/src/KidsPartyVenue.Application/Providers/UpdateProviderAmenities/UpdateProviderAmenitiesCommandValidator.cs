using FluentValidation;

namespace KidsPartyVenue.Application.Providers.UpdateProviderAmenities;

internal sealed class UpdateProviderAmenitiesCommandValidator : AbstractValidator<UpdateProviderAmenitiesCommand>
{
    public UpdateProviderAmenitiesCommandValidator()
    {
        RuleFor(c => c.ProviderId)
            .NotEmpty();

        RuleFor(c => c.AmenityIds)
            .NotNull();

        RuleForEach(c => c.AmenityIds)
            .NotEmpty();
    }
}
