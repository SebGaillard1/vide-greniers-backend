using FluentValidation;

namespace VideGreniers.Application.Events.Queries.GetNearbyEvents;

/// <summary>
/// Validator for GetNearbyEventsQuery
/// </summary>
public class GetNearbyEventsQueryValidator : AbstractValidator<GetNearbyEventsQuery>
{
    public GetNearbyEventsQueryValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90.0, 90.0)
            .WithMessage("Latitude must be between -90 and 90 degrees");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180.0, 180.0)
            .WithMessage("Longitude must be between -180 and 180 degrees");

        RuleFor(x => x.RadiusKm)
            .GreaterThan(0)
            .WithMessage("Radius must be greater than 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Radius cannot exceed 1000 km");

        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .WithMessage("Limit must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Limit cannot exceed 100");
    }
}