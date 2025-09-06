using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using VideGreniers.Application.Authentication.Models;
using VideGreniers.Application.Common.Interfaces;

namespace VideGreniers.Application.Authentication.Commands.GoogleLogin;

/// <summary>
/// Handler for GoogleLoginCommand
/// </summary>
public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, ErrorOr<AuthenticationResult>>
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<GoogleLoginCommandHandler> _logger;

    public GoogleLoginCommandHandler(
        IGoogleAuthService googleAuthService,
        IAuthenticationService authenticationService,
        ILogger<GoogleLoginCommandHandler> logger)
    {
        _googleAuthService = googleAuthService;
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<ErrorOr<AuthenticationResult>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Google OAuth login");

        // Validate Google ID token
        var googleUserInfo = await _googleAuthService.ValidateIdTokenAsync(request.IdToken);
        if (googleUserInfo == null)
        {
            _logger.LogWarning("Google token validation failed");
            return Error.Unauthorized("Auth.InvalidGoogleToken", "Invalid Google ID token.");
        }

        _logger.LogInformation("Google token validated successfully for user: {Email}", googleUserInfo.Email);

        // Authenticate user with Google info
        var result = await _authenticationService.GoogleLoginAsync(googleUserInfo);
        
        if (result.IsError)
        {
            _logger.LogWarning("Google OAuth login failed for {Email}: {Errors}", 
                googleUserInfo.Email, 
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return result.Errors;
        }

        _logger.LogInformation("User {Email} logged in successfully via Google OAuth", googleUserInfo.Email);
        return result.Value;
    }
}