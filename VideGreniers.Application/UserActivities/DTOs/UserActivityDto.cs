using VideGreniers.Domain.Enums;

namespace VideGreniers.Application.UserActivities.DTOs;

/// <summary>
/// Data transfer object for user activity
/// </summary>
public sealed record UserActivityDto(
    Guid Id,
    UserActivityType ActivityType,
    Guid? EventId,
    string? EventTitle,
    string? SearchTerm,
    string? Metadata,
    DateTime CreatedOnUtc);