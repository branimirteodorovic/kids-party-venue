namespace KidsPartyVenue.Infrastructure.Database.Seed;

/// <summary>
/// Reads the seed data files embedded in this assembly. Lines starting with <c>#</c> are comments; the first other
/// line is the header. Values never contain commas or quotes, so there is no quoting.
/// </summary>
internal static class SeedCsv
{
    public static IReadOnlyList<SeedRow> Read(string fileName)
    {
        using Stream stream = typeof(SeedCsv).Assembly.GetManifestResourceStream($"Seed.{fileName}")
            ?? throw new InvalidOperationException($"Seed file '{fileName}' is not embedded");
        using var reader = new StreamReader(stream);

        string[]? header = null;
        List<SeedRow> rows = [];
        while (reader.ReadLine() is { } line)
        {
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            if (line.Contains('"'))
            {
                throw new InvalidOperationException($"Seed file '{fileName}' has a quoted value: {line}");
            }

            string[] values = line.Split(',');
            if (header is null)
            {
                header = values;
                continue;
            }

            if (values.Length != header.Length)
            {
                throw new InvalidOperationException($"Seed file '{fileName}' has a row with {values.Length} values instead of {header.Length}: {line}");
            }

            rows.Add(new SeedRow(header.Zip(values).ToDictionary(p => p.First, p => p.Second)));
        }

        return rows;
    }
}
