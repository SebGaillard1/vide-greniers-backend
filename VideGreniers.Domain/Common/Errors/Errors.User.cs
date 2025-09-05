using ErrorOr;

namespace VideGreniers.Domain.Common.Errors;

public static partial class Errors
{
    public static class User
    {
        public static Error NotFound => Error.NotFound(
            code: "User.NotFound",
            description: "User not found.");

        public static Error InvalidEmail => Error.Validation(
            code: "User.InvalidEmail",
            description: "Invalid email address.");

        public static Error EmailAlreadyExists => Error.Conflict(
            code: "User.EmailAlreadyExists",
            description: "User with this email already exists.");

        public static Error InvalidPhoneNumber => Error.Validation(
            code: "User.InvalidPhoneNumber",
            description: "Invalid phone number format.");

        public static Error NameTooShort => Error.Validation(
            code: "User.NameTooShort",
            description: "Name must be at least 2 characters long.");

        public static Error NameTooLong => Error.Validation(
            code: "User.NameTooLong",
            description: "Name cannot exceed 100 characters.");

        public static Error ExternalAuthIdRequired => Error.Validation(
            code: "User.ExternalAuthIdRequired",
            description: "Un identifiant externe est requis pour ce type d'authentification.");
    }
}