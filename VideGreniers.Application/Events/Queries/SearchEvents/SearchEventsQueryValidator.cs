using FluentValidation;

namespace VideGreniers.Application.Events.Queries.SearchEvents;

/// <summary>
/// Validator for SearchEventsQuery
/// </summary>
public class SearchEventsQueryValidator : AbstractValidator<SearchEventsQuery>
{
    public SearchEventsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page size must be greater than or equal to 1")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .WithMessage("Search term cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));

        RuleFor(x => x.City)
            .MaximumLength(100)
            .WithMessage("City cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.EventType)
            .IsInEnum()
            .WithMessage("Event type must be valid")
            .When(x => x.EventType.HasValue);

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("Start date must be before end date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.SortBy)
            .Must(sortBy => new[] { "title", "startdate", "createdonutc", "publishedonutc" }
                .Contains(sortBy.ToLowerInvariant()))
            .WithMessage("Sort by must be one of: Title, StartDate, CreatedOnUtc, PublishedOnUtc");
    }
}