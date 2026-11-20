using FluentValidation;

namespace KidsPartyVenue.Application.Providers.RequestProviderChanges;

/// <summary>The notes are checked by <see cref="Domain.Providers.Provider"/>, so its error codes reach the client.</summary>
internal sealed class RequestProviderChangesCommandValidator : AbstractValidator<RequestProviderChangesCommand>
{
    public RequestProviderChangesCommandValidator()
    {
        RuleFor(c => c.ProviderId)
            .NotEmpty();
    }
}
