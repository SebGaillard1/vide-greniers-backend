using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;
using VideGreniers.Domain.ValueObjects;

namespace VideGreniers.Application.Events.Commands.CreateEvent;

/// <summary>
/// Handler for creating a new event
/// </summary>
public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, ErrorOr<Guid>>
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public CreateEventCommandHandler(
        IRepository<Event> eventRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        // Validate user is authenticated
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized("User must be authenticated to create events");
        }

        // Verify organizer exists
        var organizer = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (organizer == null)
        {
            return Error.NotFound("Organizer not found");
        }

        // Create the event using the Domain's static factory method
        var eventResult = Event.Create(
            title: request.Title,
            description: request.Description,
            eventType: request.EventType,
            startDate: request.StartDate,
            endDate: request.EndDate,
            latitude: request.Latitude ?? 0, // Will be geocoded if not provided
            longitude: request.Longitude ?? 0,
            street: request.Street,
            city: request.City,
            postalCode: request.PostalCode,
            country: request.Country,
            organizerId: _currentUserService.UserId.Value,
            contactPhoneNumber: request.ContactPhone,
            contactEmail: request.ContactEmail,
            specialInstructions: request.SpecialInstructions,
            entryFeeAmount: request.EntryFeeAmount,
            entryFeeCurrency: request.EntryFeeCurrency,
            categoryId: request.CategoryId,
            state: request.State);

        if (eventResult.IsError)
        {
            return eventResult.Errors;
        }

        var newEvent = eventResult.Value;

        // Configure early bird access if requested
        if (request.AllowsEarlyBird)
        {
            var earlyBirdResult = newEvent.SetEarlyBirdAccess(
                allowsEarlyBird: true,
                earlyBirdTime: request.EarlyBirdTime.HasValue ? TimeSpan.FromMinutes(request.EarlyBirdTime.Value.Hour * 60 + request.EarlyBirdTime.Value.Minute) : null,
                earlyBirdFeeAmount: request.EarlyBirdFeeAmount,
                earlyBirdFeeCurrency: request.EarlyBirdFeeCurrency ?? "EUR");

            if (earlyBirdResult.IsError)
            {
                return earlyBirdResult.Errors;
            }
        }

        // Save the event
        var createdEvent = await _eventRepository.AddAsync(newEvent, cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync("events:*", cancellationToken);

        return createdEvent.Id;
    }
}