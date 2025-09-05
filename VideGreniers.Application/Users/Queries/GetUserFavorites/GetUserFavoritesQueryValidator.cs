using FluentValidation;

namespace VideGreniers.Application.Users.Queries.GetUserFavorites;

/// <summary>
/// Validator for GetUserFavoritesQuery
/// </summary>
public class GetUserFavoritesQueryValidator : AbstractValidator<GetUserFavoritesQuery>
{
    public GetUserFavoritesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page size must be greater than or equal to 1")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100");
    }
}