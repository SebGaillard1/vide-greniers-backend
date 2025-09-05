using VideGreniers.Domain.Enums;

namespace VideGreniers.Application.Common.DTOs;

/// <summary>
/// Data transfer object for favorite information
/// </summary>
public record FavoriteDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid EventId { get; init; }
    public FavoriteStatus Status { get; init; }
    public DateTimeOffset CreatedOnUtc { get; init; }
    public DateTimeOffset? ArchivedOnUtc { get; init; }
    
    // Navigation properties
    public EventDto? Event { get; init; }
    public string? UserName { get; init; }
}