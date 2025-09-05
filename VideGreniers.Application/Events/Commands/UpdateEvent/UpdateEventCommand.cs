using ErrorOr;
using MediatR;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Application.Events.Commands.UpdateEvent;

/// <summary>
/// Command to update an existing event
/// </summary>
public record UpdateEventCommand : IRequest<ErrorOr<Updated>>
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public EventType EventType { get; init; }
    
    // Date Range
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    
    // Location
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string? State { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    
    // Contact
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public string? SpecialInstructions { get; init; }
    
    // Fees
    public decimal? EntryFeeAmount { get; init; }
    public string? EntryFeeCurrency { get; init; }
    public bool AllowsEarlyBird { get; init; }
    public TimeOnly? EarlyBirdTime { get; init; }
    public decimal? EarlyBirdFeeAmount { get; init; }
    public string? EarlyBirdFeeCurrency { get; init; }
    
    // Category
    public Guid? CategoryId { get; init; }
}