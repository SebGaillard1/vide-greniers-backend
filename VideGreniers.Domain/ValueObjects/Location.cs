using ErrorOr;
using VideGreniers.Domain.Common.Errors;

namespace VideGreniers.Domain.ValueObjects;

public sealed record Location
{
    public double Latitude { get; }
    public double Longitude { get; }

    private Location(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static ErrorOr<Location> Create(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            return Errors.Location.InvalidLatitude;
        }

        if (longitude < -180 || longitude > 180)
        {
            return Errors.Location.InvalidLongitude;
        }

        return new Location(latitude, longitude);
    }

    /// <summary>
    /// Calculate the distance in kilometers between two locations using the Haversine formula
    /// </summary>
    public double DistanceTo(Location other)
    {
        const double EarthRadiusKm = 6371.0;

        var lat1Rad = DegreesToRadians(Latitude);
        var lat2Rad = DegreesToRadians(other.Latitude);
        var deltaLatRad = DegreesToRadians(other.Latitude - Latitude);
        var deltaLngRad = DegreesToRadians(other.Longitude - Longitude);

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLngRad / 2) * Math.Sin(deltaLngRad / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    /// <summary>
    /// Check if this location is within the specified radius of another location
    /// </summary>
    public bool IsWithinRadius(Location other, double radiusKm)
    {
        return DistanceTo(other) <= radiusKm;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    public override string ToString()
    {
        return $"{Latitude:F6}, {Longitude:F6}";
    }
}