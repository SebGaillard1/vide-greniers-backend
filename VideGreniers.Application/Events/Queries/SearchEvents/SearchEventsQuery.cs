using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Behaviors;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Models;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Application.Events.Queries.SearchEvents;

/// <summary>
/// Query to search events with filters and pagination
/// </summary>
public record SearchEventsQuery : IRequest<ErrorOr<PaginatedList<EventDto>>>, ICacheableQuery
{
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public EventType? EventType { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string? City { get; init; }
    public bool? HasEntryFee { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "StartDate";
    public bool SortDescending { get; init; } = false;

    public string CacheKey => CacheKeys.EventsList(Page, PageSize, SearchTerm, CategoryId);
    public TimeSpan? CacheTime => TimeSpan.FromMinutes(5);
}