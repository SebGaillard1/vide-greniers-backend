using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;

namespace VideGreniers.Application.Notifications.Commands.CreateNotification;

/// <summary>
/// Handler for creating a new notification
/// </summary>
public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, ErrorOr<Guid>>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Event> _eventRepository;

    public CreateNotificationCommandHandler(
        IRepository<Notification> notificationRepository,
        IRepository<User> userRepository,
        IRepository<Event> eventRepository)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _eventRepository = eventRepository;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        // Verify user exists
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Error.NotFound("User not found");
        }

        // Verify event exists if eventId is provided
        if (request.EventId.HasValue)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(request.EventId.Value, cancellationToken);
            if (eventEntity == null)
            {
                return Error.NotFound("Event not found");
            }
        }

        // Create notification
        var notificationResult = Notification.Create(
            request.Title,
            request.Message,
            request.Type,
            request.UserId,
            request.EventId,
            request.ActionUrl,
            request.ActionText,
            request.ImageUrl,
            request.Metadata);

        if (notificationResult.IsError)
        {
            return notificationResult.Errors;
        }

        await _notificationRepository.AddAsync(notificationResult.Value, cancellationToken);

        return notificationResult.Value.Id;
    }
}