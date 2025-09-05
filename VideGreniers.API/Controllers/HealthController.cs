using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VideGreniers.API.Common;

namespace VideGreniers.API.Controllers;

/// <summary>
/// Health check endpoints for monitoring and diagnostics
/// </summary>
[ApiController]
[Route("[controller]")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Basic health check - indicates if the API is running
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<HealthStatus>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealth()
    {
        var result = await _healthCheckService.CheckHealthAsync();
        
        var response = new ApiResponse<object>
        {
            Data = new
            {
                Status = result.Status.ToString(),
                TotalDuration = result.TotalDuration.TotalMilliseconds,
                Entries = result.Entries.Select(kvp => new
                {
                    Name = kvp.Key,
                    Status = kvp.Value.Status.ToString(),
                    Description = kvp.Value.Description,
                    Duration = kvp.Value.Duration.TotalMilliseconds
                })
            },
            Success = result.Status == HealthStatus.Healthy,
            Timestamp = DateTime.UtcNow
        };

        var statusCode = result.Status switch
        {
            HealthStatus.Healthy => StatusCodes.Status200OK,
            HealthStatus.Degraded => StatusCodes.Status200OK,
            HealthStatus.Unhealthy => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status503ServiceUnavailable
        };

        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Readiness check - indicates if the API is ready to serve requests (database connected, etc.)
    /// </summary>
    /// <returns>Readiness status</returns>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(ApiResponse<HealthStatus>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetReadiness()
    {
        var result = await _healthCheckService.CheckHealthAsync(check => check.Tags.Contains("ready"));
        
        var response = new ApiResponse<object>
        {
            Data = new
            {
                Status = result.Status.ToString(),
                TotalDuration = result.TotalDuration.TotalMilliseconds,
                Entries = result.Entries.Select(kvp => new
                {
                    Name = kvp.Key,
                    Status = kvp.Value.Status.ToString(),
                    Description = kvp.Value.Description,
                    Duration = kvp.Value.Duration.TotalMilliseconds
                })
            },
            Success = result.Status == HealthStatus.Healthy,
            Timestamp = DateTime.UtcNow
        };

        var statusCode = result.Status == HealthStatus.Healthy 
            ? StatusCodes.Status200OK 
            : StatusCodes.Status503ServiceUnavailable;

        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Liveness check - indicates if the API process is alive (minimal dependencies)
    /// </summary>
    /// <returns>Liveness status</returns>
    [HttpGet("live")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLiveness()
    {
        var result = await _healthCheckService.CheckHealthAsync(check => check.Tags.Contains("live"));
        
        var response = new ApiResponse<object>
        {
            Data = new
            {
                Status = result.Status.ToString(),
                Timestamp = DateTime.UtcNow,
                Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "Unknown"
            },
            Success = true,
            Timestamp = DateTime.UtcNow
        };

        return Ok(response);
    }
}