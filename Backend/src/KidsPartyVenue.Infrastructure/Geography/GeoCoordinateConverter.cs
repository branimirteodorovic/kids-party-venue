using KidsPartyVenue.Domain.Geography;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;

namespace KidsPartyVenue.Infrastructure.Geography;

/// <summary>Stores a <see cref="GeoCoordinate"/> as a WGS 84 point, longitude first.</summary>
internal sealed class GeoCoordinateConverter()
    : ValueConverter<GeoCoordinate, Point>(c => ToPoint(c), p => FromPoint(p))
{
    public const string ColumnType = "geography (Point, 4326)";

    private static Point ToPoint(GeoCoordinate coordinate) =>
        new(coordinate.Longitude, coordinate.Latitude) { SRID = 4326 };

    // A geography column can't hold an out-of-range point, so a failure here is a data fault and may throw
    private static GeoCoordinate FromPoint(Point point) => GeoCoordinate.Create(point.Y, point.X).Value;
}
