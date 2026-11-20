using AwesomeAssertions;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.UnitTests.Providers;

public sealed class ProviderMembershipTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime UtcNow = CreatedAtUtc.AddDays(1);

    [Fact]
    public void Create_ShouldHaveNoMembers_WhenProviderIsUnclaimed()
    {
        // Act
        Provider provider = CreateProvider();

        // Assert
        provider.Members.Should().BeEmpty();
    }

    [Fact]
    public void AddMember_ShouldAddOwner_WhenUserIsNotAMember()
    {
        // Arrange
        Provider provider = CreateProvider();
        Guid userId = Guid.CreateVersion7();

        // Act
        Result result = provider.AddMember(userId, ProviderMemberRole.Owner, UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ProviderMember member = provider.Members.Should().ContainSingle().Subject;
        member.ProviderId.Should().Be(provider.Id);
        member.UserId.Should().Be(userId);
        member.Role.Should().Be(ProviderMemberRole.Owner);
        member.JoinedAtUtc.Should().Be(UtcNow);
        provider.UpdatedAtUtc.Should().Be(UtcNow);
    }

    [Fact]
    public void AddMember_ShouldFail_WhenUserIsAlreadyAMember()
    {
        // Arrange
        Provider provider = CreateProvider();
        Guid userId = Guid.CreateVersion7();
        provider.AddMember(userId, ProviderMemberRole.Owner, CreatedAtUtc);

        // Act
        Result result = provider.AddMember(userId, ProviderMemberRole.Owner, UtcNow);

        // Assert
        result.Error.Should().Be(ProviderErrors.AlreadyMember);
        provider.Members.Should().ContainSingle();
        provider.UpdatedAtUtc.Should().Be(CreatedAtUtc);
    }

    [Fact]
    public void AddMember_ShouldFail_WhenRoleIsUnknown()
    {
        // Arrange
        Provider provider = CreateProvider();

        // Act
        Result result = provider.AddMember(Guid.CreateVersion7(), (ProviderMemberRole)99, UtcNow);

        // Assert
        result.Error.Should().Be(ProviderErrors.UnknownMemberRole);
        provider.Members.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMember_ShouldRemoveOwner_WhenAnotherOwnerStays()
    {
        // Arrange
        Provider provider = CreateProvider();
        Guid leaving = Guid.CreateVersion7();
        Guid staying = Guid.CreateVersion7();
        provider.AddMember(leaving, ProviderMemberRole.Owner, CreatedAtUtc);
        provider.AddMember(staying, ProviderMemberRole.Owner, CreatedAtUtc);

        // Act
        Result result = provider.RemoveMember(leaving, UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Members.Should().ContainSingle().Which.UserId.Should().Be(staying);
        provider.UpdatedAtUtc.Should().Be(UtcNow);
    }

    [Fact]
    public void RemoveMember_ShouldFail_WhenUserIsTheOnlyOwner()
    {
        // Arrange
        Provider provider = CreateProvider();
        Guid owner = Guid.CreateVersion7();
        provider.AddMember(owner, ProviderMemberRole.Owner, CreatedAtUtc);

        // Act
        Result result = provider.RemoveMember(owner, UtcNow);

        // Assert
        result.Error.Should().Be(ProviderErrors.LastOwner);
        provider.Members.Should().ContainSingle();
        provider.UpdatedAtUtc.Should().Be(CreatedAtUtc);
    }

    [Fact]
    public void RemoveMember_ShouldFail_WhenUserIsNotAMember()
    {
        // Arrange
        Provider provider = CreateProvider();
        provider.AddMember(Guid.CreateVersion7(), ProviderMemberRole.Owner, CreatedAtUtc);

        // Act
        Result result = provider.RemoveMember(Guid.CreateVersion7(), UtcNow);

        // Assert
        result.Error.Should().Be(ProviderErrors.MemberNotFound);
        provider.Members.Should().ContainSingle();
    }

    private static Provider CreateProvider() =>
        Provider.Create(
            "igraonica-zeka", "Igraonica Zeka", ProviderCategory.Venue, null, "Knez Mihailova 1", Guid.CreateVersion7(),
            null, cityHasDistricts: false, GeoCoordinate.Create(44.8125, 20.4612).Value, null, isClaimed: false,
            CreatedAtUtc).Value;
}
