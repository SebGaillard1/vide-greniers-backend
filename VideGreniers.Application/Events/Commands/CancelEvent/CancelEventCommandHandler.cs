using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;

namespace VideGreniers.Application.Events.Commands.CancelEvent;

/// <summary>
/// Handler for canceling an event
/// </summary>
public class CancelEventCommandHandler : IRequestHandler<CancelEventCommand, ErrorOr<Success>>
{
    private readonly IRepository<Event> _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public CancelEventCommandHandler(
        IRepository<Event> eventRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<ErrorOr<Success>> Handle(CancelEventCommand request, CancellationToken cancellationToken)
    {
        // Validate user is authenticated
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized("User must be authenticated to cancel events");
        }

        // Validate reason
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Error.Validation("CancelEventCommand.Reason", "Cancellation reason is required");
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
            return Error.Forbidden("Only the event organizer can cancel this event");
        }

        // Cancel the event
        var cancelResult = existingEvent.Cancel(request.Reason);
        if (cancelResult.IsError)
        {
            return cancelResult.Errors;
        }

        // Save changes
        await _eventRepository.UpdateAsync(existingEvent, cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync("events:*", cancellationToken);
        await _cacheService.RemoveAsync($"events:id:{request.EventId}", cancellationToken);

        return Result.Success;
    }
}