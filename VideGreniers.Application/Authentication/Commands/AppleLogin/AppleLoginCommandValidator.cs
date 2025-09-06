using FluentValidation;

namespace VideGreniers.Application.Authentication.Commands.AppleLogin;

/// <summary>
/// Validator for AppleLoginCommand
/// </summary>
public class AppleLoginCommandValidator : AbstractValidator<AppleLoginCommand>
{
    public AppleLoginCommandValidator()
    {
        RuleFor(x => x.IdentityToken)
            .NotEmpty()
            .WithMessage("Apple identity token is required.");
    }
}