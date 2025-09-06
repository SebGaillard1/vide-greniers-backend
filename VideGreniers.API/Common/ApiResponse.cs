using System.Text.Json.Serialization;

namespace VideGreniers.API.Common;

/// <summary>
/// Standardized API response wrapper
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public record ApiResponse<T>
{
    /// <summary>
    /// Response data
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; init; }

    /// <summary>
    /// Indicates if the request was successful
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// Optional message for additional information
    /// </summary>
    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; init; }

    /// <summary>
    /// List of error messages (only present when Success is false)
    /// </summary>
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Errors { get; init; }

    /// <summary>
    /// Response timestamp
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Paginated API response
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public record PaginatedApiResponse<T> : ApiResponse<List<T>>
{
    /// <summary>
    /// Pagination metadata
    /// </summary>
    [JsonPropertyName("pagination")]
    public PaginationMetadata Pagination { get; init; } = new();
}

/// <summary>
/// Pagination metadata
/// </summary>
public record PaginationMetadata
{
    /// <summary>
    /// Current page number
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; init; }

    /// <summary>
    /// Page size
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; init; }

    /// <summary>
    /// Total number of items
    /// </summary>
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; init; }

    /// <summary>
    /// Has previous page
    /// </summary>
    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage { get; init; }

    /// <summary>
    /// Has next page
    /// </summary>
    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage { get; init; }
}