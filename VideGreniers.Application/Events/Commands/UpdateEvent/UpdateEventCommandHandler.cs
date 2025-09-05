using ErrorOr;
using MediatR;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.ValueObjects;

namespace VideGreniers.Application.Events.Commands.UpdateEvent;

/// <summary>
/// Handler for updating an existing event
/// </summary>
public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, ErrorOr<Updated>>
{
    private readonly IRepository<Event> _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public UpdateEventCommandHandler(
        IRepository<Event> eventRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<ErrorOr<Updated>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        // Validate user is authenticated
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized("User must be authenticated to update events");
        }

        // Get the event
        var existingEvent = await _eventRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existingEvent == null)
        {
            return Error.NotFound("Event not found");
        }

        // Verify ownership
        if (existingEvent.OrganizerId != _currentUserService.UserId.Value)
        {
            return Error.Forbidden("Only the event organizer can update this event");
        }

        // Update event details
        var updateDetailsResult = existingEvent.UpdateDetails(
            title: request.Title,
            description: request.Description,
            eventType: request.EventType,
            startDate: request.StartDate,
            endDate: request.EndDate,
            specialInstructions: request.SpecialInstructions,
            categoryId: request.CategoryId);

        if (updateDetailsResult.IsError)
        {
            return updateDetailsResult.Errors;
        }

        // Update location
        var updateLocationResult = existingEvent.UpdateLocation(
            latitude: request.Latitude ?? existingEvent.Location.Latitude,
            longitude: request.Longitude ?? existingEvent.Location.Longitude,
            street: request.Street,
            city: request.City,
            postalCode: request.PostalCode,
            country: request.Country,
            state: request.State);

        if (updateLocationResult.IsError)
        {
            return updateLocationResult.Errors;
        }

        // Update contact information
        var updateContactResult = existingEvent.UpdateContactInformation(
            contactPhoneNumber: request.ContactPhone,
            contactEmail: request.ContactEmail);

        if (updateContactResult.IsError)
        {
            return updateContactResult.Errors;
        }

        // Update entry fee
        var setEntryFeeResult = existingEvent.SetEntryFee(
            amount: request.EntryFeeAmount,
            currency: request.EntryFeeCurrency ?? "EUR");

        if (setEntryFeeResult.IsError)
        {
            return setEntryFeeResult.Errors;
        }

        // Update early bird access
        var setEarlyBirdResult = existingEvent.SetEarlyBirdAccess(
            allowsEarlyBird: request.AllowsEarlyBird,
            earlyBirdTime: request.EarlyBirdTime.HasValue ? TimeSpan.FromMinutes(request.EarlyBirdTime.Value.Hour * 60 + request.EarlyBirdTime.Value.Minute) : null,
            earlyBirdFeeAmount: request.EarlyBirdFeeAmount,
            earlyBirdFeeCurrency: request.EarlyBirdFeeCurrency ?? "EUR");

        var updateResult = setEarlyBirdResult;

        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        // Save changes
        await _eventRepository.UpdateAsync(existingEvent, cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveByPatternAsync("events:*", cancellationToken);
        await _cacheService.RemoveAsync($"events:id:{request.Id}", cancellationToken);

        return Result.Updated;
    }
}