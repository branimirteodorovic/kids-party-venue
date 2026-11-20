using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using KidsPartyVenue.Application.Providers.GetMyProviders;
using KidsPartyVenue.Application.Providers.GetProviderForEditing;
using KidsPartyVenue.Domain.BookingPolicies;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Venues;
using KidsPartyVenue.IntegrationTests.Abstractions;
using KidsPartyVenue.IntegrationTests.Users;

namespace KidsPartyVenue.IntegrationTests.Providers;

public sealed class ProviderReviewWorkflowTests(IntegrationTestWebAppFactory factory) : ProviderEditorTestBase(factory)
{
    [Fact]
    public async Task Approve_ShouldPublishProfile_WhenOwnerSubmittedCompleteProfile()
    {
        // Arrange
        (Guid providerId, LoggedInUser owner) = await RegisterCompleteProviderAsync();
        ProviderForEditingResponse draft = await GetAsAdminAsync(providerId);
        (await GetPublicStatusAsync(draft.Slug)).Should().Be(HttpStatusCode.NotFound);

        // Act
        uint version = await ReadVersionAsync(await SubmitAsync(providerId, draft.Version, owner.AccessToken));
        ProviderForEditingResponse pending = await GetAsAdminAsync(providerId);
        HttpResponseMessage response = await AdminActionAsync(providerId, "approve", new { version });

        // Assert
        pending.Status.Should().Be(ProviderStatus.PendingReview);
        pending.SubmittedAtUtc.Should().NotBeNull();
        uint approvedVersion = await ReadVersionAsync(response);
        ProviderForEditingResponse published = await GetAsAdminAsync(providerId);
        published.Status.Should().Be(ProviderStatus.Published);
        published.ReviewedAtUtc.Should().NotBeNull();
        published.Version.Should().Be(approvedVersion);
        (await GetPublicStatusAsync(draft.Slug)).Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RequestChanges_ShouldSendProfileBackWithNotes_UntilItIsResubmittedAndApproved()
    {
        // Arrange
        (Guid providerId, LoggedInUser owner) = await RegisterCompleteProviderAsync();
        uint version = await ReadVersionAsync(await SubmitAsync(providerId, (await GetAsAdminAsync(providerId)).Version, owner.AccessToken));

        // Act
        version = await ReadVersionAsync(await AdminActionAsync(providerId, "request-changes", new { notes = "  Dodajte opis  ", version }));
        ProviderForEditingResponse sentBack = await GetAsOwnerAsync(providerId, owner.AccessToken);
        version = await ReadVersionAsync(await SendAsync(
            HttpMethod.Put, $"providers/{providerId}/booking-policy", Policy(version), owner.AccessToken));
        version = await ReadVersionAsync(await SubmitAsync(providerId, version, owner.AccessToken));
        ProviderForEditingResponse resubmitted = await GetAsAdminAsync(providerId);
        await ReadVersionAsync(await AdminActionAsync(providerId, "approve", new { version }));

        // Assert
        sentBack.Status.Should().Be(ProviderStatus.ChangesRequested);
        sentBack.ReviewNotes.Should().Be("Dodajte opis");
        resubmitted.Status.Should().Be(ProviderStatus.PendingReview);
        resubmitted.ReviewNotes.Should().Be("Dodajte opis");
        ProviderForEditingResponse published = await GetAsAdminAsync(providerId);
        published.Status.Should().Be(ProviderStatus.Published);
        published.ReviewNotes.Should().BeNull();
    }

    [Fact]
    public async Task RequestChanges_ShouldReturnBadRequest_WhenNotesAreBlank()
    {
        // Arrange
        Guid providerId = await CreateSubmittedProviderAsync();

        // Act
        HttpResponseMessage response = await AdminActionAsync(
            providerId, "request-changes", new { notes = "  ", (await GetAsAdminAsync(providerId)).Version });

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, ProviderErrors.ReviewNotesRequired.Code);
    }

    [Fact]
    public async Task Submit_ShouldReturnBadRequest_WhenProfileIsIncomplete()
    {
        // Arrange: the admin-created draft has a phone but no policy or details
        Guid providerId = await CreateProviderAsync();

        // Act
        HttpResponseMessage response = await SubmitAsync(providerId, (await GetAsAdminAsync(providerId)).Version, await AdminTokenAsync());

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, ProviderErrors.BookingPolicyRequired.Code);
        (await GetAsAdminAsync(providerId)).Status.Should().Be(ProviderStatus.Draft);
    }

    [Fact]
    public async Task Submit_ShouldReturnBadRequest_WhenPhoneIsMissing()
    {
        // Arrange
        (Guid providerId, LoggedInUser owner) = await RegisterCompleteProviderAsync(phone: null);

        // Act
        HttpResponseMessage response = await SubmitAsync(providerId, (await GetAsAdminAsync(providerId)).Version, owner.AccessToken);

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, ProviderErrors.PhoneRequired.Code);
    }

    [Fact]
    public async Task Submit_ShouldReturnConflict_WhenAlreadyPending()
    {
        // Arrange
        Guid providerId = await CreateSubmittedProviderAsync();

        // Act
        HttpResponseMessage response = await SubmitAsync(providerId, (await GetAsAdminAsync(providerId)).Version, await AdminTokenAsync());

        // Assert
        await ShouldFailWith(response, HttpStatusCode.Conflict, ProviderErrors.NotSubmittable.Code);
    }

    [Fact]
    public async Task Approve_ShouldReturnConflict_WhenProfileIsDraft()
    {
        // Arrange
        Guid providerId = await CreateCompleteProviderAsync();

        // Act
        HttpResponseMessage response = await AdminActionAsync(providerId, "approve", new { (await GetAsAdminAsync(providerId)).Version });

        // Assert
        await ShouldFailWith(response, HttpStatusCode.Conflict, ProviderErrors.NotPendingReview.Code);
    }

    [Theory]
    [InlineData("basic-info")]
    [InlineData("venue-details")]
    [InlineData("booking-policy")]
    [InlineData("amenities")]
    public async Task Update_ShouldReturnConflict_WhileProfileIsPendingReview(string section)
    {
        // Arrange
        Guid providerId = await CreateSubmittedProviderAsync();
        ProviderForEditingResponse current = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(providerId, section, ValidRequest(section, current));

        // Assert
        await ShouldFailWith(response, HttpStatusCode.Conflict, ProviderErrors.InReview.Code);
    }

    [Fact]
    public async Task Update_ShouldSucceed_WhenPublishedProfileIsSuspended()
    {
        // Arrange
        Guid providerId = await CreatePublishedProviderAsync();
        await ReadVersionAsync(await AdminActionAsync(
            providerId, "suspend", new { reason = "Provera", (await GetAsAdminAsync(providerId)).Version }));
        ProviderForEditingResponse current = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(providerId, "booking-policy", ValidRequest("booking-policy", current));

        // Assert
        await ReadVersionAsync(response);
    }

    [Fact]
    public async Task Suspend_ShouldHidePublicPage_UntilUnsuspended()
    {
        // Arrange
        Guid providerId = await CreatePublishedProviderAsync();
        ProviderForEditingResponse published = await GetAsAdminAsync(providerId);
        (await GetPublicStatusAsync(published.Slug)).Should().Be(HttpStatusCode.OK);

        // Act
        uint version = await ReadVersionAsync(await AdminActionAsync(
            providerId, "suspend", new { reason = " Prijava roditelja ", published.Version }));
        HttpStatusCode whileSuspended = await GetPublicStatusAsync(published.Slug);
        ProviderForEditingResponse suspended = await GetAsAdminAsync(providerId);
        await ReadVersionAsync(await AdminActionAsync(providerId, "unsuspend", new { version }));

        // Assert
        whileSuspended.Should().Be(HttpStatusCode.NotFound);
        suspended.Status.Should().Be(ProviderStatus.Published);
        suspended.SuspendedAtUtc.Should().NotBeNull();
        suspended.SuspensionReason.Should().Be("Prijava roditelja");
        ProviderForEditingResponse restored = await GetAsAdminAsync(providerId);
        restored.SuspendedAtUtc.Should().BeNull();
        restored.SuspensionReason.Should().BeNull();
        (await GetPublicStatusAsync(published.Slug)).Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Suspend_ShouldReturnConflict_WhenAlreadySuspended()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        uint version = await ReadVersionAsync(await AdminActionAsync(
            providerId, "suspend", new { reason = "Prvi", (await GetAsAdminAsync(providerId)).Version }));

        // Act
        HttpResponseMessage response = await AdminActionAsync(providerId, "suspend", new { reason = "Drugi", version });

        // Assert
        await ShouldFailWith(response, HttpStatusCode.Conflict, ProviderErrors.AlreadySuspended.Code);
    }

    [Fact]
    public async Task Suspend_ShouldReturnBadRequest_WhenReasonIsMissing()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();

        // Act
        HttpResponseMessage response = await AdminActionAsync(
            providerId, "suspend", new { reason = "", (await GetAsAdminAsync(providerId)).Version });

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, ProviderErrors.SuspensionReasonRequired.Code);
    }

    [Fact]
    public async Task Unsuspend_ShouldReturnConflict_WhenNotSuspended()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();

        // Act
        HttpResponseMessage response = await AdminActionAsync(providerId, "unsuspend", new { (await GetAsAdminAsync(providerId)).Version });

        // Assert
        await ShouldFailWith(response, HttpStatusCode.Conflict, ProviderErrors.NotSuspended.Code);
    }

    [Theory]
    [InlineData("submit")]
    [InlineData("approve")]
    [InlineData("request-changes")]
    [InlineData("suspend")]
    [InlineData("unsuspend")]
    public async Task Transition_ShouldReturnConflict_WhenVersionIsStale(string action)
    {
        // Arrange: a profile in the status each action starts from
        Guid providerId = action switch
        {
            "submit" => await CreateCompleteProviderAsync(),
            "approve" or "request-changes" => await CreateSubmittedProviderAsync(),
            "unsuspend" => await CreateSuspendedProviderAsync(),
            _ => await CreateProviderAsync()
        };
        uint stale = (await GetAsAdminAsync(providerId)).Version - 1;
        object request = new { notes = "Dodajte opis", reason = "Provera", version = stale };

        // Act
        HttpResponseMessage response = action == "submit"
            ? await SubmitAsync(providerId, stale, await AdminTokenAsync())
            : await AdminActionAsync(providerId, action, request);

        // Assert
        await ShouldFailWith(response, HttpStatusCode.Conflict, ProviderErrors.ConcurrencyConflict.Code);
    }

    [Fact]
    public async Task GetMyProviders_ShouldReturnSuspendedFlag()
    {
        // Arrange
        (Guid providerId, LoggedInUser owner) = await RegisterCompleteProviderAsync();
        await ReadVersionAsync(await AdminActionAsync(
            providerId, "suspend", new { reason = "Provera", (await GetAsAdminAsync(providerId)).Version }));

        // Act
        HttpResponseMessage response = await SendAsync(HttpMethod.Get, "users/me/providers", null, owner.AccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<MyProviderResponse> providers = (await response.Content.ReadFromJsonAsync<List<MyProviderResponse>>(JsonOptions))!;
        providers.Should().ContainSingle().Which.IsSuspended.Should().BeTrue();
    }

    [Theory]
    [InlineData("approve")]
    [InlineData("request-changes")]
    [InlineData("suspend")]
    [InlineData("unsuspend")]
    public async Task AdminAction_ShouldReturnForbidden_WhenUserIsOwner(string action)
    {
        // Arrange
        (Guid providerId, LoggedInUser owner) = await RegisterCompleteProviderAsync();

        // Act
        HttpResponseMessage response = await SendAsync(
            HttpMethod.Post, $"admin/providers/{providerId}/{action}",
            new { notes = "x", reason = "x", (await GetAsAdminAsync(providerId)).Version }, owner.AccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminAction_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Act
        HttpResponseMessage response = await SendAsync(
            HttpMethod.Post, $"admin/providers/{Guid.CreateVersion7()}/approve", new { version = 1 }, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminAction_ShouldReturnNotFound_WhenProviderDoesNotExist()
    {
        // Act
        HttpResponseMessage response = await AdminActionAsync(Guid.CreateVersion7(), "approve", new { version = 1 });

        // Assert
        await ShouldFailWith(response, HttpStatusCode.NotFound, ProviderErrors.NotFound.Code);
    }

    [Fact]
    public async Task Submit_ShouldReturnForbidden_WhenUserOwnsAnotherProvider()
    {
        // Arrange
        (_, LoggedInUser owner) = await RegisterCompleteProviderAsync();
        Guid otherProviderId = await CreateCompleteProviderAsync();

        // Act
        HttpResponseMessage response = await SubmitAsync(otherProviderId, (await GetAsAdminAsync(otherProviderId)).Version, owner.AccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Submit_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Act
        HttpResponseMessage response = await SubmitAsync(Guid.CreateVersion7(), 1, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A new parent registers a venue in Novi Sad (open for venues) and fills in its policy and details.</summary>
    private async Task<(Guid ProviderId, LoggedInUser Owner)> RegisterCompleteProviderAsync(string? phone = "063 123 4567")
    {
        LoggedInUser owner = await HttpClient.RegisterAndLoginAsync(Faker);
        District district = await GetNoviSadDistrictAsync();
        HttpResponseMessage registered = await SendAsync(HttpMethod.Post, "providers", new
        {
            name = UniqueName(),
            category = "Venue",
            address = Faker.Address.StreetAddress(),
            cityId = district.CityId,
            districtId = district.Id,
            latitude = Latitude,
            longitude = Longitude,
            phone
        }, owner.AccessToken);
        registered.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid providerId = (await registered.Content.ReadFromJsonAsync<IdResponse>(JsonOptions))!.Id;

        await FillInAsync(providerId, owner.AccessToken);

        return (providerId, owner);
    }

    private async Task<Guid> CreateCompleteProviderAsync()
    {
        Guid providerId = await CreateProviderAsync();
        await FillInAsync(providerId, await AdminTokenAsync());

        return providerId;
    }

    private async Task<Guid> CreateSubmittedProviderAsync()
    {
        Guid providerId = await CreateCompleteProviderAsync();
        await ReadVersionAsync(await SubmitAsync(providerId, (await GetAsAdminAsync(providerId)).Version, await AdminTokenAsync()));

        return providerId;
    }

    private async Task<Guid> CreatePublishedProviderAsync()
    {
        Guid providerId = await CreateSubmittedProviderAsync();
        await ReadVersionAsync(await AdminActionAsync(providerId, "approve", new { (await GetAsAdminAsync(providerId)).Version }));

        return providerId;
    }

    private async Task<Guid> CreateSuspendedProviderAsync()
    {
        Guid providerId = await CreateProviderAsync();
        await ReadVersionAsync(await AdminActionAsync(
            providerId, "suspend", new { reason = "Provera", (await GetAsAdminAsync(providerId)).Version }));

        return providerId;
    }

    private async Task FillInAsync(Guid providerId, string accessToken)
    {
        ProviderForEditingResponse current = await GetAsOwnerAsync(providerId, accessToken);
        uint version = await ReadVersionAsync(await SendAsync(
            HttpMethod.Put, $"providers/{providerId}/venue-details", Details(current.Version), accessToken));
        await ReadVersionAsync(await SendAsync(HttpMethod.Put, $"providers/{providerId}/booking-policy", Policy(version), accessToken));
    }

    private async Task<ProviderForEditingResponse> GetAsOwnerAsync(Guid providerId, string accessToken)
    {
        HttpResponseMessage response = await SendAsync(HttpMethod.Get, $"providers/{providerId}", null, accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<ProviderForEditingResponse>(JsonOptions))!;
    }

    private Task<HttpResponseMessage> SubmitAsync(Guid providerId, uint version, string? accessToken) =>
        SendAsync(HttpMethod.Post, $"providers/{providerId}/submit", new { version }, accessToken);

    private async Task<HttpResponseMessage> AdminActionAsync(Guid providerId, string action, object request) =>
        await SendAsync(HttpMethod.Post, $"admin/providers/{providerId}/{action}", request, await AdminTokenAsync());

    private async Task<HttpStatusCode> GetPublicStatusAsync(string slug) =>
        (await HttpClient.GetAsync($"providers/{slug}")).StatusCode;

    private static object ValidRequest(string section, ProviderForEditingResponse current) =>
        section switch
        {
            "basic-info" => new
            {
                current.Name, current.Description, current.Address, current.CityId, current.DistrictId,
                current.Latitude, current.Longitude, current.Phone, current.Version
            },
            "venue-details" => Details(current.Version),
            "booking-policy" => Policy(current.Version),
            "amenities" => new { amenityIds = new List<Guid>(), current.Version },
            _ => throw new ArgumentOutOfRangeException(nameof(section))
        };

    private static object Details(uint version) => new
    {
        maxChildren = 30,
        minAge = 3,
        maxAge = 10,
        outsideCatering = OutsideFoodRule.NotAllowed,
        outsideCake = OutsideFoodRule.Allowed,
        foodService = FoodService.Included,
        hasOwnAnimator = true,
        hasOwnPhotographer = false,
        hasSmokingArea = false,
        version
    };

    private static object Policy(uint version) => new
    {
        depositType = DepositType.None,
        freeCancellationDays = 7,
        version
    };

    private sealed record IdResponse(Guid Id);
}
