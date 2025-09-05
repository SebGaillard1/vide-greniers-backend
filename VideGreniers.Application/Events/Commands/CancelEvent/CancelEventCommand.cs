using ErrorOr;
using MediatR;

namespace VideGreniers.Application.Events.Commands.CancelEvent;

/// <summary>
/// Command to cancel an event
/// </summary>
public record CancelEventCommand(Guid EventId, string Reason) : IRequest<ErrorOr<Success>>;