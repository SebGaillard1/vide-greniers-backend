using FluentValidation;

namespace VideGreniers.Application.Users.Commands.ToggleFavorite;

/// <summary>
/// Validator for ToggleFavoriteCommand
/// </summary>
public class ToggleFavoriteCommandValidator : AbstractValidator<ToggleFavoriteCommand>
{
    public ToggleFavoriteCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event ID is required");
    }
}