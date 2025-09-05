namespace VideGreniers.Application.Common.DTOs;

/// <summary>
/// Data transfer object for location information
/// </summary>
public record LocationDto
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string? State { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}