using FluentValidation;

namespace KidsPartyVenue.Application.Providers.UpdateVenueDetails;

/// <summary>The ranges are checked by <see cref="Domain.Venues.VenueDetails.Create"/>, so its error codes reach the client.</summary>
internal sealed class UpdateVenueDetailsCommandValidator : AbstractValidator<UpdateVenueDetailsCommand>
{
    public UpdateVenueDetailsCommandValidator()
    {
        RuleFor(c => c.ProviderId)
            .NotEmpty();
    }
}
