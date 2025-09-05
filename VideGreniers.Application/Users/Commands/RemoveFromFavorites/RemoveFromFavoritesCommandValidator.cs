using FluentValidation;

namespace VideGreniers.Application.Users.Commands.RemoveFromFavorites;

/// <summary>
/// Validator for RemoveFromFavoritesCommand
/// </summary>
public class RemoveFromFavoritesCommandValidator : AbstractValidator<RemoveFromFavoritesCommand>
{
    public RemoveFromFavoritesCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event ID is required");
    }
}