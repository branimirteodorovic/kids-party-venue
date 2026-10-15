namespace KidsPartyVenue.Infrastructure.Identity;

public sealed class RefreshToken
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid FamilyId { get; init; }          // all tokens descending from one login
    public string TokenHash { get; init; } = string.Empty;  // SHA-256 hex, unique index
    public DateTime CreatedAtUtc { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public DateTime? RevokedAtUtc { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public uint Version { get; set; }            // maps to xmin, optimistic concurrency
}
