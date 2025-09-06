using FluentValidation;

namespace VideGreniers.Application.Authentication.Commands.GoogleLogin;

/// <summary>
/// Validator for GoogleLoginCommand
/// </summary>
public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty()
            .WithMessage("Google ID token is required.");
    }
}