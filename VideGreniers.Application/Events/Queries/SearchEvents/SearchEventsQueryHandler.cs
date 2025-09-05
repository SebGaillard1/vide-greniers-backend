using AutoMapper;
using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Extensions;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Application.Common.Models;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Specifications;

namespace VideGreniers.Application.Events.Queries.SearchEvents;

/// <summary>
/// Handler for searching events with filters and pagination
/// </summary>
public class SearchEventsQueryHandler : IRequestHandler<SearchEventsQuery, ErrorOr<PaginatedList<EventDto>>>
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IMapper _mapper;

    public SearchEventsQueryHandler(
        IRepository<Event> eventRepository,
        IMapper mapper)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<PaginatedList<EventDto>>> Handle(SearchEventsQuery request, CancellationToken cancellationToken)
    {
        // Validate page size
        if (request.PageSize > 100)
        {
            return Error.Validation("SearchEventsQuery.PageSize", "Page size cannot exceed 100");
        }

        // Build the specification for complex filtering
        var specification = new ComplexEventFilterSpecification(
            userLocation: null, // Location filtering handled separately
            radiusKm: null,
            startDate: request.StartDate,
            endDate: request.EndDate,
            eventType: request.EventType,
            categoryId: request.CategoryId,
            hasEntryFee: request.HasEntryFee,
            searchTerm: request.SearchTerm,
            pageNumber: request.Page,
            pageSize: request.PageSize);

        var events = await _eventRepository.GetAsync(specification, cancellationToken);

        // Apply additional city filtering if specified
        if (!string.IsNullOrWhiteSpace(request.City))
        {
            events = events.Where(e => 
                e.Address.City.Contains(request.City, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Count total results for pagination (before pagination is applied)
        var countSpecification = new ComplexEventFilterSpecification(
            userLocation: null,
            radiusKm: null,
            startDate: request.StartDate,
            endDate: request.EndDate,
            eventType: request.EventType,
            categoryId: request.CategoryId,
            hasEntryFee: request.HasEntryFee,
            searchTerm: request.SearchTerm);

        var totalCount = await _eventRepository.CountAsync(countSpecification, cancellationToken);

        // Apply city filter to count as well
        if (!string.IsNullOrWhiteSpace(request.City))
        {
            var allEvents = await _eventRepository.GetAsync(countSpecification, cancellationToken);
            totalCount = allEvents.Count(e => 
                e.Address.City.Contains(request.City, StringComparison.OrdinalIgnoreCase));
        }

        // Apply sorting
        events = request.SortBy.ToLowerInvariant() switch
        {
            "title" => request.SortDescending 
                ? events.OrderByDescending(e => e.Title).ToList()
                : events.OrderBy(e => e.Title).ToList(),
            "createdonutc" => request.SortDescending
                ? events.OrderByDescending(e => e.CreatedOnUtc).ToList()
                : events.OrderBy(e => e.CreatedOnUtc).ToList(),
            "publishedonutc" => request.SortDescending
                ? events.OrderByDescending(e => e.PublishedOnUtc).ToList()
                : events.OrderBy(e => e.PublishedOnUtc).ToList(),
            _ => request.SortDescending
                ? events.OrderByDescending(e => e.DateRange.StartDate).ToList()
                : events.OrderBy(e => e.DateRange.StartDate).ToList()
        };

        // Map to DTOs
        var eventDtos = events.Select(e => e.ToDto()).ToList();

        // Create paginated result
        var paginatedResult = PaginatedList<EventDto>.Create(
            eventDtos,
            totalCount,
            request.Page,
            request.PageSize);

        return paginatedResult;
    }
}