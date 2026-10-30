using FluentValidation;

namespace KidsPartyVenue.Application.Users.ChangePreferredLanguage;

internal sealed class ChangePreferredLanguageCommandValidator : AbstractValidator<ChangePreferredLanguageCommand>
{
    public ChangePreferredLanguageCommandValidator()
    {
        RuleFor(c => c.Language).IsInEnum();
    }
}
