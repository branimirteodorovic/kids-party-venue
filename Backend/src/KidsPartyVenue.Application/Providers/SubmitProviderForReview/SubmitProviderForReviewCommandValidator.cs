using FluentValidation;

namespace KidsPartyVenue.Application.Providers.SubmitProviderForReview;

internal sealed class SubmitProviderForReviewCommandValidator : AbstractValidator<SubmitProviderForReviewCommand>
{
    public SubmitProviderForReviewCommandValidator()
    {
        RuleFor(c => c.ProviderId)
            .NotEmpty();
    }
}
