using ErrorOr;

namespace VideGreniers.Domain.Common.Errors;

public static partial class Errors
{
    public static class Favorite
    {
        public static Error InvalidUserId => Error.Validation(
            code: "Favorite.InvalidUserId",
            description: "L'identifiant utilisateur est invalide.");

        public static Error InvalidEventId => Error.Validation(
            code: "Favorite.InvalidEventId",
            description: "L'identifiant de l'événement est invalide.");

        public static Error AlreadyExists => Error.Conflict(
            code: "Favorite.AlreadyExists",
            description: "Cet événement est déjà dans les favoris.");

        public static Error NotFound => Error.NotFound(
            code: "Favorite.NotFound",
            description: "Favori introuvable.");
    }
}