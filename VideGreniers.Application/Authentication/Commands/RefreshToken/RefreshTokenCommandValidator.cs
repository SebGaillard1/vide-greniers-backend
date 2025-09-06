using FluentValidation;

namespace VideGreniers.Application.Authentication.Commands.RefreshToken;

/// <summary>
/// Validator for RefreshTokenCommand
/// </summary>
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty()
            .WithMessage("Access token is required");

        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required")
            .MinimumLength(10)
            .WithMessage("Refresh token format is invalid");

        RuleFor(x => x.DeviceInfo)
            .MaximumLength(500)
            .WithMessage("Device info cannot exceed 500 characters");
    }
}