namespace VideGreniers.Application.Common.DTOs;

/// <summary>
/// Data transfer object for user information
/// </summary>
public record UserDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public DateTimeOffset CreatedOnUtc { get; init; }
    public DateTimeOffset? ModifiedOnUtc { get; init; }
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public int CreatedEventsCount { get; init; }
    public int FavoritesCount { get; init; }
}