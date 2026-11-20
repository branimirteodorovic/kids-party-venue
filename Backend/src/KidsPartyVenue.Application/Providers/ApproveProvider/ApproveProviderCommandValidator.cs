using FluentValidation;

namespace KidsPartyVenue.Application.Providers.ApproveProvider;

internal sealed class ApproveProviderCommandValidator : AbstractValidator<ApproveProviderCommand>
{
    public ApproveProviderCommandValidator()
    {
        RuleFor(c => c.ProviderId)
            .NotEmpty();
    }
}
