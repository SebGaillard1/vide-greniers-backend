using ErrorOr;

namespace VideGreniers.Domain.Common.Errors;

public static partial class Errors
{
    public static class DateRange
    {
        public static Error EndDateMustBeAfterStartDate => Error.Validation(
            code: "DateRange.EndDateMustBeAfterStartDate",
            description: "La date de fin doit être après la date de début.");

        public static Error StartDateCannotBeInPast => Error.Validation(
            code: "DateRange.StartDateCannotBeInPast",
            description: "La date de début ne peut pas être dans le passé.");
    }
}