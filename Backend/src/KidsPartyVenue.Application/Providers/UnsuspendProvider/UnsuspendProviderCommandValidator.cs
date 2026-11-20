using FluentValidation;

namespace KidsPartyVenue.Application.Providers.UnsuspendProvider;

internal sealed class UnsuspendProviderCommandValidator : AbstractValidator<UnsuspendProviderCommand>
{
    public UnsuspendProviderCommandValidator()
    {
        RuleFor(c => c.ProviderId)
            .NotEmpty();
    }
}
