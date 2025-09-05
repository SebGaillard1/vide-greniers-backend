using VideGreniers.Domain.Enums;

namespace VideGreniers.Application.Common.Models;

/// <summary>
/// Parameters for event search and filtering
/// </summary>
public record SearchParameters
{
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public EventType? EventType { get; init; }
    public EventStatus? Status { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public int? RadiusKm { get; init; }
    public bool? HasEntryFee { get; init; }
    public string? City { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "StartDate";
    public bool SortDescending { get; init; } = false;
}