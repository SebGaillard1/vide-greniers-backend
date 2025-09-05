using MediatR;
using Microsoft.Extensions.Logging;
using VideGreniers.Application.Common.Interfaces;
using System.Diagnostics;

namespace VideGreniers.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for performance monitoring
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();

        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _currentUserService.UserId;
            var userEmail = _currentUserService.Email;

            _logger.LogWarning(
                "Long running request detected: {RequestName} ({ElapsedMilliseconds} ms) by user {UserId} ({UserEmail})",
                requestName,
                elapsedMilliseconds,
                userId,
                userEmail);
        }

        return response;
    }
}