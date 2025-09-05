using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;

namespace VideGreniers.Application.Events.Commands.PublishEvent;

/// <summary>
/// Handler for publishing an event
/// </summary>
public class PublishEventCommandHandler : IRequestHandler<PublishEventCommand, ErrorOr<Success>>
{
    private readonly IRepository<Event> _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICacheService _cacheService;

    public PublishEventCommandHandler(
        IRepository<Event> eventRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        ICacheService cacheService)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _cacheService = cacheService;
    }

    public async Task<ErrorOr<Success>> Handle(PublishEventCommand request, CancellationToken cancellationToken)
    {
        // Validate user is authenticated
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized("User must be authenticated to publish events");
        }

        // Get the event
        var existingEvent = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);
        if (existingEvent == null)
        {
            return Error.NotFound("Event not found");
        }

        // Verify ownership
        if (existingEvent.OrganizerId != _currentUserService.UserId.Value)
        {
            return Error.Forbidden("Only the event organizer can publish this event");
        }

        // Publish the event
        var publishResult = existingEvent.Publish();
        if (publishResult.IsError)
        {
            return publishResult.Errors;
        }

        // Save changes
        await _eventRepository.UpdateAsync(existingEvent, cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync("events:*", cancellationToken);
        await _cacheService.RemoveAsync($"events:id:{request.EventId}", cancellationToken);

        return Result.Success;
    }
}