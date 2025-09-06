using Serilog;
using System.Diagnostics;

namespace VideGreniers.API.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with performance timing
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;

        // Add correlation ID to response headers
        context.Response.Headers.TryAdd("X-Correlation-ID", correlationId);

        var stopwatch = Stopwatch.StartNew();
        
        // Log request
        _logger.LogInformation(
            "HTTP {RequestMethod} {RequestPath} started with correlation ID {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log response
            var logLevel = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            
            _logger.Log(logLevel,
                "HTTP {RequestMethod} {RequestPath} completed with status {StatusCode} in {ElapsedMilliseconds}ms - Correlation ID {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);

            // Add response time header (only if response hasn't started)
            if (!context.Response.HasStarted)
            {
                context.Response.Headers.TryAdd("X-Response-Time-Ms", stopwatch.ElapsedMilliseconds.ToString());
            }

            // Log slow requests
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning(
                    "Slow request detected: {RequestMethod} {RequestPath} took {ElapsedMilliseconds}ms - Correlation ID {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    correlationId);
            }
        }
    }
}