using AwesomeAssertions;
using FluentValidation;
using FluentValidation.Results;
using KidsPartyVenue.Application.Providers.ApproveProvider;
using KidsPartyVenue.Application.Providers.RequestProviderChanges;
using KidsPartyVenue.Application.Providers.SubmitProviderForReview;
using KidsPartyVenue.Application.Providers.SuspendProvider;
using KidsPartyVenue.Application.Providers.UnsuspendProvider;

namespace KidsPartyVenue.UnitTests.Providers;

/// <summary>The five review commands only check the provider ID; notes and reasons are checked by the domain.</summary>
public sealed class ReviewCommandValidatorTests
{
    public static TheoryData<string> Commands => ["submit", "approve", "request-changes", "suspend", "unsuspend"];

    [Theory]
    [MemberData(nameof(Commands))]
    public void Validate_ShouldPass_WhenProviderIdIsSet(string command)
    {
        // Act
        ValidationResult result = Validate(command, Guid.CreateVersion7());

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(Commands))]
    public void Validate_ShouldFail_WhenProviderIdIsEmpty(string command)
    {
        // Act
        ValidationResult result = Validate(command, Guid.Empty);

        // Assert
        result.Errors.Should().ContainSingle().Which.PropertyName.Should().Be("ProviderId");
    }

    [Fact]
    public void Validate_ShouldPass_WhenNotesAndReasonAreBlank()
    {
        // Act
        ValidationResult notes = new RequestProviderChangesCommandValidator().Validate(
            new RequestProviderChangesCommand(Guid.CreateVersion7(), "", 1));
        ValidationResult reason = new SuspendProviderCommandValidator().Validate(
            new SuspendProviderCommand(Guid.CreateVersion7(), "", 1));

        // Assert: the domain returns Providers.ReviewNotesRequired and Providers.SuspensionReasonRequired instead
        notes.IsValid.Should().BeTrue();
        reason.IsValid.Should().BeTrue();
    }

    private static ValidationResult Validate(string command, Guid providerId) =>
        command switch
        {
            "submit" => Run(new SubmitProviderForReviewCommandValidator(), new SubmitProviderForReviewCommand(providerId, 1)),
            "approve" => Run(new ApproveProviderCommandValidator(), new ApproveProviderCommand(providerId, 1)),
            "request-changes" => Run(new RequestProviderChangesCommandValidator(), new RequestProviderChangesCommand(providerId, "Dodajte opis", 1)),
            "suspend" => Run(new SuspendProviderCommandValidator(), new SuspendProviderCommand(providerId, "Provera", 1)),
            "unsuspend" => Run(new UnsuspendProviderCommandValidator(), new UnsuspendProviderCommand(providerId, 1)),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };

    private static ValidationResult Run<T>(AbstractValidator<T> validator, T instance) => validator.Validate(instance);
}
