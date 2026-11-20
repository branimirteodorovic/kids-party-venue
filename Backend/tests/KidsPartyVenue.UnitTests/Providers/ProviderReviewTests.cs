using AwesomeAssertions;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.BookingPolicies;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Phones;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Venues;

namespace KidsPartyVenue.UnitTests.Providers;

public sealed class ProviderReviewTests
{
    private static readonly DateTime UtcNow = DateTime.UtcNow;
    private static readonly DateTime Later = UtcNow.AddHours(1);
    private static readonly PhoneNumber Phone = PhoneNumber.Create("064 123 4567").Value;
    private static readonly GeoCoordinate Location = GeoCoordinate.Create(45.2389, 19.8441).Value;
    private static readonly BookingPolicy Policy = BookingPolicy.Create(DepositType.None, null, null, 7).Value;

    [Fact]
    public void SubmitForReview_ShouldMoveToPendingReview_WhenDraftIsComplete()
    {
        // Arrange
        Provider provider = CompleteProvider();

        // Act
        Result result = provider.SubmitForReview(Later);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Status.Should().Be(ProviderStatus.PendingReview);
        provider.SubmittedAtUtc.Should().Be(Later);
        provider.UpdatedAtUtc.Should().Be(Later);
    }

    [Fact]
    public void SubmitForReview_ShouldKeepNotes_WhenChangesWereRequested()
    {
        // Arrange
        Provider provider = Pending();
        provider.RequestChanges("Dodajte fotografije", UtcNow);

        // Act
        Result result = provider.SubmitForReview(Later);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Status.Should().Be(ProviderStatus.PendingReview);
        provider.ReviewNotes.Should().Be("Dodajte fotografije");
    }

    [Fact]
    public void SubmitForReview_ShouldFail_WhenAlreadyPending()
    {
        // Arrange
        Provider provider = Pending();

        // Act
        Result result = provider.SubmitForReview(Later);

        // Assert
        result.Error.Should().Be(ProviderErrors.NotSubmittable);
    }

    [Fact]
    public void SubmitForReview_ShouldFail_WhenPublished()
    {
        // Arrange
        Provider provider = Published();

        // Act
        Result result = provider.SubmitForReview(Later);

        // Assert
        result.Error.Should().Be(ProviderErrors.NotSubmittable);
    }

    [Fact]
    public void SubmitForReview_ShouldFailWithBookingPolicyFirst_WhenNothingIsSet()
    {
        // Arrange
        Provider provider = CreateProvider(withPhone: false);

        // Act
        Result result = provider.SubmitForReview(Later);

        // Assert
        result.Error.Should().Be(ProviderErrors.BookingPolicyRequired);
        provider.Status.Should().Be(ProviderStatus.Draft);
        provider.SubmittedAtUtc.Should().BeNull();
    }

    [Fact]
    public void SubmitForReview_ShouldFailWithVenueDetails_WhenPolicyIsSetButDetailsAreNot()
    {
        // Arrange
        Provider provider = CreateProvider(withPhone: false);
        provider.ChangeBookingPolicy(Policy, UtcNow);

        // Act
        Result result = provider.SubmitForReview(Later);

        // Assert
        result.Error.Should().Be(ProviderErrors.VenueDetailsRequired);
    }

    [Fact]
    public void SubmitForReview_ShouldFailWithPhone_WhenOnlyPhoneIsMissing()
    {
        // Arrange
        Provider provider = CreateProvider(withPhone: false);
        provider.ChangeBookingPolicy(Policy, UtcNow);
        provider.ChangeVenueDetails(CreateVenueDetails(), UtcNow);

        // Act
        Result result = provider.SubmitForReview(Later);

        // Assert
        result.Error.Should().Be(ProviderErrors.PhoneRequired);
    }

    [Fact]
    public void SubmitForReview_ShouldNotRequireVenueDetails_WhenProviderIsNotVenue()
    {
        // Arrange
        Provider provider = CreateProvider(category: ProviderCategory.Catering);
        provider.ChangeBookingPolicy(Policy, UtcNow);

        // Act
        Result result = provider.SubmitForReview(Later);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Approve_ShouldPublishAndClearNotes_WhenPending()
    {
        // Arrange
        Provider provider = Pending();
        provider.RequestChanges("Dodajte opis", UtcNow);
        provider.SubmitForReview(UtcNow);

        // Act
        Result result = provider.Approve(Later);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Status.Should().Be(ProviderStatus.Published);
        provider.ReviewNotes.Should().BeNull();
        provider.ReviewedAtUtc.Should().Be(Later);
        provider.UpdatedAtUtc.Should().Be(Later);
    }

    [Fact]
    public void Approve_ShouldFail_WhenDraft()
    {
        // Arrange
        Provider provider = CompleteProvider();

        // Act
        Result result = provider.Approve(Later);

        // Assert
        result.Error.Should().Be(ProviderErrors.NotPendingReview);
        provider.Status.Should().Be(ProviderStatus.Draft);
    }

    [Fact]
    public void RequestChanges_ShouldStoreTrimmedNotes_WhenPending()
    {
        // Arrange
        Provider provider = Pending();

        // Act
        Result result = provider.RequestChanges("  Dodajte opis  ", Later);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Status.Should().Be(ProviderStatus.ChangesRequested);
        provider.ReviewNotes.Should().Be("Dodajte opis");
        provider.ReviewedAtUtc.Should().Be(Later);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RequestChanges_ShouldFail_WhenNotesAreBlank(string? notes)
    {
        // Arrange
        Provider provider = Pending();

        // Act
        Result result = provider.RequestChanges(notes!, Later);

        // Assert
        result.Error.Should().Be(ProviderErrors.ReviewNotesRequired);
        provider.Status.Should().Be(ProviderStatus.PendingReview);
    }

    [Fact]
    public void RequestChanges_ShouldAcceptNotes_WhenAtMaxLength()
    {
        // Arrange
        Provider provider = Pending();

        // Act
        Result result = provider.RequestChanges(new string('a', Provider.ReviewNotesMaxLength), Later);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RequestChanges_ShouldFail_WhenNotesAreTooLong()
    {
        // Arrange
        Provider provider = Pending();

        // Act
        Result result = provider.RequestChanges(new string('a', Provider.ReviewNotesMaxLength + 1), Later);

        // Assert
        result.Error.Should().Be(ProviderErrors.ReviewNotesTooLong);
    }

    [Fact]
    public void RequestChanges_ShouldFail_WhenPublished()
    {
        // Arrange
        Provider provider = Published();

        // Act
        Result result = provider.RequestChanges("Dodajte opis", Later);

        // Assert
        result.Error.Should().Be(ProviderErrors.NotPendingReview);
    }

    [Fact]
    public void Changes_ShouldFailWithInReview_WhenPending()
    {
        // Arrange
        Provider provider = Pending();

        // Act
        Result basicInfo = provider.ChangeBasicInfo(
            "Novo ime", null, "Adresa 1", provider.CityId, null, cityHasDistricts: false, Location, Phone, Later);
        Result policy = provider.ChangeBookingPolicy(Policy, Later);
        Result details = provider.ChangeVenueDetails(CreateVenueDetails(), Later);
        Result amenities = provider.ChangeAmenities([], Later);

        // Assert
        basicInfo.Error.Should().Be(ProviderErrors.InReview);
        policy.Error.Should().Be(ProviderErrors.InReview);
        details.Error.Should().Be(ProviderErrors.InReview);
        amenities.Error.Should().Be(ProviderErrors.InReview);
        provider.UpdatedAtUtc.Should().Be(UtcNow);
    }

    [Fact]
    public void Changes_ShouldSucceed_WhenPublishedAndSuspended()
    {
        // Arrange
        Provider provider = Published();
        provider.Suspend("Prijava roditelja", UtcNow);

        // Act
        Result policy = provider.ChangeBookingPolicy(Policy, Later);
        Result details = provider.ChangeVenueDetails(CreateVenueDetails(), Later);

        // Assert
        policy.IsSuccess.Should().BeTrue();
        details.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Suspend_ShouldSetReasonAndKeepStatus_WhenNotSuspended()
    {
        // Arrange
        Provider provider = Published();

        // Act
        Result result = provider.Suspend("  Prijava roditelja ", Later);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.SuspendedAtUtc.Should().Be(Later);
        provider.SuspensionReason.Should().Be("Prijava roditelja");
        provider.Status.Should().Be(ProviderStatus.Published);
        provider.UpdatedAtUtc.Should().Be(Later);
    }

    [Fact]
    public void Suspend_ShouldFail_WhenAlreadySuspended()
    {
        // Arrange
        Provider provider = Published();
        provider.Suspend("Prvi razlog", UtcNow);

        // Act
        Result result = provider.Suspend("Drugi razlog", Later);

        // Assert
        result.Error.Should().Be(ProviderErrors.AlreadySuspended);
        provider.SuspensionReason.Should().Be("Prvi razlog");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Suspend_ShouldFail_WhenReasonIsBlank(string? reason)
    {
        // Arrange
        Provider provider = Published();

        // Act
        Result result = provider.Suspend(reason!, Later);

        // Assert
        result.Error.Should().Be(ProviderErrors.SuspensionReasonRequired);
        provider.SuspendedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Suspend_ShouldAcceptReason_WhenAtMaxLength()
    {
        // Arrange
        Provider provider = Published();

        // Act
        Result result = provider.Suspend(new string('a', Provider.SuspensionReasonMaxLength), Later);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Suspend_ShouldFail_WhenReasonIsTooLong()
    {
        // Arrange
        Provider provider = Published();

        // Act
        Result result = provider.Suspend(new string('a', Provider.SuspensionReasonMaxLength + 1), Later);

        // Assert
        result.Error.Should().Be(ProviderErrors.SuspensionReasonTooLong);
    }

    [Fact]
    public void Unsuspend_ShouldClearSuspension_WhenSuspended()
    {
        // Arrange
        Provider provider = Published();
        provider.Suspend("Prijava roditelja", UtcNow);

        // Act
        Result result = provider.Unsuspend(Later);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.SuspendedAtUtc.Should().BeNull();
        provider.SuspensionReason.Should().BeNull();
        provider.Status.Should().Be(ProviderStatus.Published);
        provider.UpdatedAtUtc.Should().Be(Later);
    }

    [Fact]
    public void Unsuspend_ShouldFail_WhenNotSuspended()
    {
        // Arrange
        Provider provider = Published();

        // Act
        Result result = provider.Unsuspend(Later);

        // Assert
        result.Error.Should().Be(ProviderErrors.NotSuspended);
    }

    [Fact]
    public void Approve_ShouldPublishButKeepSuspension_WhenSuspendedWhilePending()
    {
        // Arrange
        Provider provider = Pending();
        provider.Suspend("Provera", UtcNow);

        // Act
        Result result = provider.Approve(Later);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Status.Should().Be(ProviderStatus.Published);
        provider.SuspendedAtUtc.Should().NotBeNull();
    }

    private static Provider Published()
    {
        Provider provider = Pending();
        provider.Approve(UtcNow).IsSuccess.Should().BeTrue();

        return provider;
    }

    private static Provider Pending()
    {
        Provider provider = CompleteProvider();
        provider.SubmitForReview(UtcNow).IsSuccess.Should().BeTrue();

        return provider;
    }

    private static Provider CompleteProvider()
    {
        Provider provider = CreateProvider();
        provider.ChangeBookingPolicy(Policy, UtcNow);
        provider.ChangeVenueDetails(CreateVenueDetails(), UtcNow);

        return provider;
    }

    private static Provider CreateProvider(ProviderCategory category = ProviderCategory.Venue, bool withPhone = true) =>
        Provider.Create(
            "igraonica-zeka", "Igraonica Zeka", category, null, "Bulevar oslobođenja 1", Guid.CreateVersion7(), null,
            cityHasDistricts: false, Location, withPhone ? Phone : null, isClaimed: true, UtcNow).Value;

    private static VenueDetails CreateVenueDetails() =>
        VenueDetails.Create(
            30, null, 3, 10, OutsideFoodRule.NotAllowed, OutsideFoodRule.Allowed, FoodService.ForPurchase,
            hasOwnAnimator: true, hasOwnPhotographer: false, hasSmokingArea: false).Value;
}
