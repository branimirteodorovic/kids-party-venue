using FluentValidation;

namespace KidsPartyVenue.Application.Providers.UpdateBookingPolicy;

/// <summary>The terms are checked by <see cref="Domain.BookingPolicies.BookingPolicy.Create"/>, so its error codes reach the client.</summary>
internal sealed class UpdateBookingPolicyCommandValidator : AbstractValidator<UpdateBookingPolicyCommand>
{
    public UpdateBookingPolicyCommandValidator()
    {
        RuleFor(c => c.ProviderId)
            .NotEmpty();
    }
}
