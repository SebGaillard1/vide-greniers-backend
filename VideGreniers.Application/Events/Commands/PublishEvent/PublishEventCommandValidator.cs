using FluentValidation;

namespace VideGreniers.Application.Events.Commands.PublishEvent;

/// <summary>
/// Validator for PublishEventCommand
/// </summary>
public class PublishEventCommandValidator : AbstractValidator<PublishEventCommand>
{
    public PublishEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event ID is required");
    }
}