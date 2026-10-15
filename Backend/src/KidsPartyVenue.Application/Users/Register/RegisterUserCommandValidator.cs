using FluentValidation;

namespace KidsPartyVenue.Application.Users.Register;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(c => c.FullName)
            .NotEmpty()
            .MaximumLength(200);
    }
}
