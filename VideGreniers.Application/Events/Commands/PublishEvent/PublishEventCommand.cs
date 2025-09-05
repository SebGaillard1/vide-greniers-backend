using ErrorOr;
using MediatR;

namespace VideGreniers.Application.Events.Commands.PublishEvent;

/// <summary>
/// Command to publish a draft event
/// </summary>
public record PublishEventCommand(Guid EventId) : IRequest<ErrorOr<Success>>;