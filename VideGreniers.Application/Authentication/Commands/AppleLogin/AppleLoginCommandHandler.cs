using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using VideGreniers.Application.Authentication.Models;
using VideGreniers.Application.Common.Interfaces;

namespace VideGreniers.Application.Authentication.Commands.AppleLogin;

/// <summary>
/// Handler for AppleLoginCommand
/// </summary>
public class AppleLoginCommandHandler : IRequestHandler<AppleLoginCommand, ErrorOr<AuthenticationResult>>
{
    private readonly IAppleAuthService _appleAuthService;
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AppleLoginCommandHandler> _logger;

    public AppleLoginCommandHandler(
        IAppleAuthService appleAuthService,
        IAuthenticationService authenticationService,
        ILogger<AppleLoginCommandHandler> logger)
    {
        _appleAuthService = appleAuthService;
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<ErrorOr<AuthenticationResult>> Handle(AppleLoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Apple Sign In");

        // Validate Apple identity token
        var appleUserInfo = await _appleAuthService.ValidateIdentityTokenAsync(request.IdentityToken);
        if (appleUserInfo == null)
        {
            _logger.LogWarning("Apple token validation failed");
            return Error.Unauthorized("Auth.InvalidAppleToken", "Invalid Apple identity token.");
        }

        // If user name is provided from the client (first time login), use it
        // Apple only provides name on the first authentication
        if (!string.IsNullOrEmpty(request.UserFirstName) || !string.IsNullOrEmpty(request.UserLastName))
        {
            appleUserInfo = appleUserInfo with 
            { 
                FirstName = request.UserFirstName ?? appleUserInfo.FirstName,
                LastName = request.UserLastName ?? appleUserInfo.LastName
            };
        }

        _logger.LogInformation("Apple token validated successfully for user: {Email}", appleUserInfo.Email);

        // Authenticate user with Apple info
        var result = await _authenticationService.AppleLoginAsync(appleUserInfo);
        
        if (result.IsError)
        {
            _logger.LogWarning("Apple Sign In failed for {Email}: {Errors}", 
                appleUserInfo.Email, 
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return result.Errors;
        }

        _logger.LogInformation("User {Email} logged in successfully via Apple Sign In", appleUserInfo.Email);
        return result.Value;
    }
}