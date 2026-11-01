using System.Globalization;

namespace KidsPartyVenue.Infrastructure.Database.Seed;

/// <summary>One row of a seed file, read by column name.</summary>
internal sealed class SeedRow(IReadOnlyDictionary<string, string> values)
{
    public string Text(string column) => values[column];

    public string? OptionalText(string column) => values[column] is { Length: > 0 } value ? value : null;

    public Guid Guid(string column) => System.Guid.Parse(values[column], CultureInfo.InvariantCulture);

    public int Int(string column) => int.Parse(values[column], CultureInfo.InvariantCulture);

    public double Double(string column) => double.Parse(values[column], CultureInfo.InvariantCulture);
}
