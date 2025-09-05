using ErrorOr;
using VideGreniers.Domain.Common.Errors;
using VideGreniers.Domain.Common.Models;

namespace VideGreniers.Domain.ValueObjects;

/// <summary>
/// Location value object following the exact requirements from the prompt
/// </summary>
public sealed class LocationWithAddress : ValueObject
{
    public string Address { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Region { get; }
    public string Country { get; }
    public double Latitude { get; }
    public double Longitude { get; }
    
    private LocationWithAddress(
        string address,
        string city,
        string postalCode,
        string region,
        string country,
        double latitude,
        double longitude)
    {
        Address = address;
        City = city;
        PostalCode = postalCode;
        Region = region;
        Country = country;
        Latitude = latitude;
        Longitude = longitude;
    }
    
    public static ErrorOr<LocationWithAddress> Create(
        string address,
        string city,
        string postalCode,
        string region,
        string country,
        double latitude,
        double longitude)
    {
        // Validations
        if (string.IsNullOrWhiteSpace(address))
            return Errors.Location.InvalidAddress;
        
        if (string.IsNullOrWhiteSpace(city))
            return Errors.Location.InvalidCity;
            
        if (latitude < -90 || latitude > 90)
            return Errors.Location.InvalidLatitude;
            
        if (longitude < -180 || longitude > 180)
            return Errors.Location.InvalidLongitude;
        
        return new LocationWithAddress(address, city, postalCode, region, country, latitude, longitude);
    }
    
    // Calculer la distance entre deux locations (formule Haversine)
    public double CalculateDistanceInKm(LocationWithAddress other)
    {
        const double earthRadiusKm = 6371;
        
        var latRad1 = Latitude * Math.PI / 180;
        var latRad2 = other.Latitude * Math.PI / 180;
        var deltaLat = (other.Latitude - Latitude) * Math.PI / 180;
        var deltaLon = (other.Longitude - Longitude) * Math.PI / 180;
        
        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(latRad1) * Math.Cos(latRad2) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return earthRadiusKm * c;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Address;
        yield return City;
        yield return PostalCode;
        yield return Region;
        yield return Country;
        yield return Latitude;
        yield return Longitude;
    }
}