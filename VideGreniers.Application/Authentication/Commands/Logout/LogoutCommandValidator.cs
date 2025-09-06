using FluentValidation;

namespace VideGreniers.Application.Authentication.Commands.Logout;

/// <summary>
/// Validator for LogoutCommand
/// </summary>
public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required")
            .MinimumLength(10)
            .WithMessage("Refresh token format is invalid");
    }
}