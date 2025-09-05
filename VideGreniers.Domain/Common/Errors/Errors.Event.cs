using ErrorOr;

namespace VideGreniers.Domain.Common.Errors;

public static partial class Errors
{
    public static class Event
    {
        public static Error NotFound => Error.NotFound(
            code: "Event.NotFound",
            description: "Event not found.");

        public static Error TitleTooShort => Error.Validation(
            code: "Event.TitleTooShort",
            description: "Event title must be at least 3 characters long.");

        public static Error TitleTooLong => Error.Validation(
            code: "Event.TitleTooLong",
            description: "Event title cannot exceed 200 characters.");

        public static Error DescriptionTooLong => Error.Validation(
            code: "Event.DescriptionTooLong",
            description: "Event description cannot exceed 2000 characters.");

        public static Error InvalidDateRange => Error.Validation(
            code: "Event.InvalidDateRange",
            description: "End date must be after start date.");

        public static Error StartDateInPast => Error.Validation(
            code: "Event.StartDateInPast",
            description: "Event start date cannot be in the past.");

        public static Error InvalidLocation => Error.Validation(
            code: "Event.InvalidLocation",
            description: "Invalid location coordinates.");

        public static Error CannotModifyPublishedEvent => Error.Conflict(
            code: "Event.CannotModifyPublishedEvent",
            description: "Cannot modify a published event.");

        public static Error CannotCancelCompletedEvent => Error.Conflict(
            code: "Event.CannotCancelCompletedEvent",
            description: "Cannot cancel a completed event.");

        public static Error AlreadyCancelled => Error.Conflict(
            code: "Event.AlreadyCancelled",
            description: "Event is already cancelled.");

        public static Error UnauthorizedAccess => Error.Forbidden(
            code: "Event.UnauthorizedAccess",
            description: "You are not authorized to access this event.");

        public static Error InvalidTitle => Error.Validation(
            code: "Event.InvalidTitle",
            description: "Le titre doit contenir entre 3 et 100 caractères.");

        public static Error InvalidDescription => Error.Validation(
            code: "Event.InvalidDescription",
            description: "La description doit contenir entre 10 et 2000 caractères.");

        public static Error InvalidContactEmail => Error.Validation(
            code: "Event.InvalidContactEmail",
            description: "L'email de contact est invalide.");

        public static Error CannotPublish => Error.Conflict(
            code: "Event.CannotPublish",
            description: "Cet événement ne peut pas être publié dans son état actuel.");

        public static Error CannotCancel => Error.Conflict(
            code: "Event.CannotCancel",
            description: "Cet événement ne peut pas être annulé.");

        public static Error EventDateHasPassed => Error.Conflict(
            code: "Event.EventDateHasPassed",
            description: "La date de l'événement est dépassée.");

        public static Error CannotUpdateClosedEvent => Error.Conflict(
            code: "Event.CannotUpdateClosedEvent",
            description: "Un événement terminé ou annulé ne peut pas être modifié.");
    }
}