using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace VideGreniers.API.Common;

/// <summary>
/// Base API controller with MediatR integration and ErrorOr result handling
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    private ISender? _mediator;
    
    /// <summary>
    /// MediatR sender for CQRS commands and queries
    /// </summary>
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Handles ErrorOr results and converts them to appropriate HTTP responses
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="result">ErrorOr result from application layer</param>
    /// <returns>HTTP action result</returns>
    protected IActionResult HandleResult<T>(ErrorOr<T> result)
    {
        if (result.IsError)
        {
            return HandleError(result.Errors);
        }

        return Ok(new ApiResponse<T>
        {
            Data = result.Value,
            Success = true,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Handles successful results without data
    /// </summary>
    /// <param name="result">ErrorOr success result</param>
    /// <returns>HTTP action result</returns>
    protected IActionResult HandleResult(ErrorOr<Success> result)
    {
        if (result.IsError)
        {
            return HandleError(result.Errors);
        }

        return Ok(new ApiResponse<object>
        {
            Data = null,
            Success = true,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Handles successful results for Updated operations
    /// </summary>
    /// <param name="result">ErrorOr updated result</param>
    /// <returns>HTTP action result</returns>
    protected IActionResult HandleResult(ErrorOr<Updated> result)
    {
        if (result.IsError)
        {
            return HandleError(result.Errors);
        }

        return Ok(new ApiResponse<object>
        {
            Data = null,
            Success = true,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Handles successful results for Created operations
    /// </summary>
    /// <param name="result">ErrorOr created result with ID</param>
    /// <param name="actionName">Action name for location header</param>
    /// <param name="routeValues">Route values for location header</param>
    /// <returns>HTTP action result</returns>
    protected IActionResult HandleCreatedResult<T>(ErrorOr<T> result, string? actionName = null, object? routeValues = null)
    {
        if (result.IsError)
        {
            return HandleError(result.Errors);
        }

        var response = new ApiResponse<T>
        {
            Data = result.Value,
            Success = true,
            Timestamp = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(actionName))
        {
            return CreatedAtAction(actionName, routeValues, response);
        }

        return Created(string.Empty, response);
    }

    /// <summary>
    /// Converts ErrorOr errors to appropriate HTTP responses
    /// </summary>
    /// <param name="errors">List of errors</param>
    /// <returns>HTTP action result</returns>
    private IActionResult HandleError(List<Error> errors)
    {
        var firstError = errors.First();

        var errorResponse = new ApiResponse<object>
        {
            Data = null,
            Success = false,
            Errors = errors.Select(e => e.Description).ToList(),
            Timestamp = DateTime.UtcNow
        };

        return firstError.Type switch
        {
            ErrorType.NotFound => NotFound(errorResponse),
            ErrorType.Validation => BadRequest(errorResponse),
            ErrorType.Conflict => Conflict(errorResponse),
            ErrorType.Unauthorized => Unauthorized(errorResponse),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, errorResponse),
            _ => StatusCode(StatusCodes.Status500InternalServerError, errorResponse)
        };
    }
}