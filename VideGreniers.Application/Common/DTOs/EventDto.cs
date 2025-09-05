using VideGreniers.Domain.Enums;

namespace VideGreniers.Application.Common.DTOs;

/// <summary>
/// Data transfer object for event information
/// </summary>
public record EventDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public LocationDto Location { get; init; } = new();
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public EventStatus Status { get; init; }
    public EventType EventType { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public string? SpecialInstructions { get; init; }
    public decimal? EntryFeeAmount { get; init; }
    public string? EntryFeeCurrency { get; init; }
    public bool AllowsEarlyBird { get; init; }
    public TimeOnly? EarlyBirdTime { get; init; }
    public decimal? EarlyBirdFeeAmount { get; init; }
    public string? EarlyBirdFeeCurrency { get; init; }
    public DateTimeOffset? PublishedOnUtc { get; init; }
    public DateTimeOffset CreatedOnUtc { get; init; }
    public DateTimeOffset? ModifiedOnUtc { get; init; }
    
    // Navigation properties
    public string? OrganizerName { get; init; }
    public string? CategoryName { get; init; }
    public string? CategoryIcon { get; init; }
    public string? CategoryColor { get; init; }
    
    // Computed properties
    public double? DistanceKm { get; init; }
    public bool? IsFavorite { get; init; }
    public int FavoriteCount { get; init; }
}