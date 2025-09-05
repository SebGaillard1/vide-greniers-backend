using FluentValidation;

namespace VideGreniers.Application.Users.Commands.AddToFavorites;

/// <summary>
/// Validator for AddToFavoritesCommand
/// </summary>
public class AddToFavoritesCommandValidator : AbstractValidator<AddToFavoritesCommand>
{
    public AddToFavoritesCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event ID is required");
    }
}