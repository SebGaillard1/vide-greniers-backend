using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using VideGreniers.Application.Common.Interfaces;

namespace VideGreniers.Application.Authentication.Commands.Logout;

/// <summary>
/// Handler for LogoutCommand
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ErrorOr<bool>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IAuthenticationService authenticationService,
        ILogger<LogoutCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<ErrorOr<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing logout request");

        var result = await _authenticationService.LogoutAsync(request.RefreshToken);

        if (result.IsError)
        {
            _logger.LogWarning("Logout failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return result.Errors;
        }

        _logger.LogInformation("User logged out successfully");
        return result.Value;
    }
}