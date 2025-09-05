using ErrorOr;

namespace VideGreniers.Domain.Common.Errors;

public static partial class Errors
{
    public static class Location
    {
        public static Error InvalidLatitude => Error.Validation(
            code: "Location.InvalidLatitude",
            description: "Latitude must be between -90 and 90 degrees.");

        public static Error InvalidLongitude => Error.Validation(
            code: "Location.InvalidLongitude",
            description: "Longitude must be between -180 and 180 degrees.");

        public static Error InvalidRadius => Error.Validation(
            code: "Location.InvalidRadius",
            description: "Search radius must be between 1 and 100 kilometers.");

        public static Error InvalidAddress => Error.Validation(
            code: "Location.InvalidAddress",
            description: "L'adresse est invalide.");

        public static Error InvalidCity => Error.Validation(
            code: "Location.InvalidCity",
            description: "La ville est invalide.");
    }
}