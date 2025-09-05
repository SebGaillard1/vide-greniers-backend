using FluentValidation;

namespace VideGreniers.Application.Events.Commands.CancelEvent;

/// <summary>
/// Validator for CancelEventCommand
/// </summary>
public class CancelEventCommandValidator : AbstractValidator<CancelEventCommand>
{
    public CancelEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Cancellation reason is required")
            .Length(10, 500)
            .WithMessage("Cancellation reason must be between 10 and 500 characters");
    }
}