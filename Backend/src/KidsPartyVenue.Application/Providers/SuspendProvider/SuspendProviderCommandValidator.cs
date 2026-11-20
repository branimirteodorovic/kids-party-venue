using FluentValidation;

namespace KidsPartyVenue.Application.Providers.SuspendProvider;

/// <summary>The reason is checked by <see cref="Domain.Providers.Provider"/>, so its error codes reach the client.</summary>
internal sealed class SuspendProviderCommandValidator : AbstractValidator<SuspendProviderCommand>
{
    public SuspendProviderCommandValidator()
    {
        RuleFor(c => c.ProviderId)
            .NotEmpty();
    }
}
