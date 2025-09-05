using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Behaviors;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Models;

namespace VideGreniers.Application.Events.Queries.GetEventById;

/// <summary>
/// Query to get an event by its ID
/// </summary>
public record GetEventByIdQuery(Guid EventId) : IRequest<ErrorOr<EventDto>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.EventById(EventId);
    public TimeSpan? CacheTime => TimeSpan.FromMinutes(10);
}