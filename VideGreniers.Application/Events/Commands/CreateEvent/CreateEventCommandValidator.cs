using FluentValidation;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Application.Events.Commands.CreateEvent;

/// <summary>
/// Validator for CreateEventCommand
/// </summary>
public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .Length(3, 200)
            .WithMessage("Title must be between 3 and 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .Length(10, 2000)
            .WithMessage("Description must be between 10 and 2000 characters");

        RuleFor(x => x.EventType)
            .IsInEnum()
            .WithMessage("Event type is required and must be valid");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required")
            .GreaterThan(DateTimeOffset.UtcNow.AddHours(-1))
            .WithMessage("Start date must be in the future");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        // Location validation
        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Street address is required")
            .MaximumLength(200)
            .WithMessage("Street address cannot exceed 200 characters");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaximumLength(100)
            .WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithMessage("Postal code is required")
            .MaximumLength(20)
            .WithMessage("Postal code cannot exceed 20 characters");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required")
            .MaximumLength(100)
            .WithMessage("Country cannot exceed 100 characters");

        RuleFor(x => x.State)
            .MaximumLength(100)
            .WithMessage("State cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.State));

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90.0, 90.0)
            .WithMessage("Latitude must be between -90 and 90 degrees")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180.0, 180.0)
            .WithMessage("Longitude must be between -180 and 180 degrees")
            .When(x => x.Longitude.HasValue);

        // Contact validation
        RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .WithMessage("Contact email must be a valid email address")
            .MaximumLength(256)
            .WithMessage("Contact email cannot exceed 256 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20)
            .WithMessage("Contact phone cannot exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactPhone));

        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(1000)
            .WithMessage("Special instructions cannot exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.SpecialInstructions));

        // Fee validation
        RuleFor(x => x.EntryFeeAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Entry fee amount must be non-negative")
            .When(x => x.EntryFeeAmount.HasValue);

        RuleFor(x => x.EntryFeeCurrency)
            .NotEmpty()
            .WithMessage("Entry fee currency is required when entry fee amount is specified")
            .Length(3)
            .WithMessage("Currency code must be exactly 3 characters (ISO 4217)")
            .When(x => x.EntryFeeAmount.HasValue);

        // Early bird validation
        When(x => x.AllowsEarlyBird, () =>
        {
            RuleFor(x => x.EarlyBirdTime)
                .NotNull()
                .WithMessage("Early bird time is required when early bird is enabled");

            RuleFor(x => x.EarlyBirdFeeAmount)
                .NotNull()
                .WithMessage("Early bird fee amount is required when early bird is enabled")
                .GreaterThanOrEqualTo(0)
                .WithMessage("Early bird fee amount must be non-negative");

            RuleFor(x => x.EarlyBirdFeeCurrency)
                .NotEmpty()
                .WithMessage("Early bird fee currency is required when early bird is enabled")
                .Length(3)
                .WithMessage("Currency code must be exactly 3 characters (ISO 4217)");
        });
    }
}