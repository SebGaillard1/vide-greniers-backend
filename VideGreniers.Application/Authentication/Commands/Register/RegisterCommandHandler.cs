using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using VideGreniers.Application.Authentication.Models;
using VideGreniers.Application.Common.Interfaces;

namespace VideGreniers.Application.Authentication.Commands.Register;

/// <summary>
/// Handler for RegisterCommand
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<AuthenticationResult>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IAuthenticationService authenticationService,
        ILogger<RegisterCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<ErrorOr<AuthenticationResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing registration for email: {Email}", request.Email);

        var result = await _authenticationService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        if (result.IsError)
        {
            _logger.LogWarning("Registration failed for {Email}: {Errors}", request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return result.Errors;
        }

        _logger.LogInformation("User {Email} registered successfully", request.Email);
        return result.Value;
    }
}