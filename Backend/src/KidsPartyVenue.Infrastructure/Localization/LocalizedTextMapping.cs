using KidsPartyVenue.Domain.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsPartyVenue.Infrastructure.Localization;

/// <summary>
/// Maps <see cref="LocalizedText"/> as an owned type in its owner's table. An owned type rather than a complex type,
/// because <c>HasData</c> can't seed complex types.
/// </summary>
internal static class LocalizedTextMapping
{
    public const int LookupNameMaxLength = 100;

    /// <summary>Maps the text to one column per language: <c>{prefix}_sr</c>, <c>{prefix}_en</c> and <c>{prefix}_ru</c>.</summary>
    public static OwnedNavigationBuilder<TOwner, LocalizedText> MapLocalizedText<TOwner>(
        this OwnedNavigationBuilder<TOwner, LocalizedText> builder,
        string columnPrefix,
        int maxLength)
        where TOwner : class
    {
        builder.Property(t => t.Sr).HasColumnName($"{columnPrefix}_sr").HasMaxLength(maxLength);
        builder.Property(t => t.En).HasColumnName($"{columnPrefix}_en").HasMaxLength(maxLength);
        builder.Property(t => t.Ru).HasColumnName($"{columnPrefix}_ru").HasMaxLength(maxLength);

        return builder;
    }
}
