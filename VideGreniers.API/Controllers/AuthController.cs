using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VideGreniers.API.Common;
using VideGreniers.Application.Authentication.Commands.AppleLogin;
using VideGreniers.Application.Authentication.Commands.GoogleLogin;
using VideGreniers.Application.Authentication.Commands.Login;
using VideGreniers.Application.Authentication.Commands.Logout;
using VideGreniers.Application.Authentication.Commands.RefreshToken;
using VideGreniers.Application.Authentication.Commands.Register;
using VideGreniers.Application.Authentication.Queries.GetCurrentUser;

namespace VideGreniers.API.Controllers;

/// <summary>
/// Authentication endpoints for user management
/// </summary>
[Route("api/auth")]
public class AuthController : ApiController
{
    /// <summary>
    /// Register a new user with email and password
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            GetDeviceInfo());

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password,
            GetDeviceInfo());

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(
            request.AccessToken,
            request.RefreshToken,
            GetDeviceInfo());

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Logout and revoke refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var command = new LogoutCommand(request.RefreshToken);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var query = new GetCurrentUserQuery();
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// OAuth login with Google
    /// </summary>
    [HttpPost("oauth/google")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var command = new GoogleLoginCommand(
            request.IdToken,
            GetDeviceInfo());

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// OAuth login with Apple Sign In
    /// </summary>
    [HttpPost("oauth/apple")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> AppleLogin([FromBody] AppleLoginRequest request)
    {
        var command = new AppleLoginCommand(
            request.IdentityToken,
            request.UserFirstName,
            request.UserLastName,
            GetDeviceInfo());

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Get device information from request headers
    /// </summary>
    private string GetDeviceInfo()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        return $"{userAgent} - {ipAddress}";
    }

}

/// <summary>
/// Register request DTO
/// </summary>
public record RegisterRequest(
    string Email,
    string Password,
    string? FirstName,
    string? LastName);

/// <summary>
/// Login request DTO
/// </summary>
public record LoginRequest(
    string Email,
    string Password);

/// <summary>
/// Refresh token request DTO
/// </summary>
public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken);

/// <summary>
/// Logout request DTO
/// </summary>
public record LogoutRequest(string RefreshToken);

/// <summary>
/// Google login request DTO
/// </summary>
public record GoogleLoginRequest(string IdToken);

/// <summary>
/// Apple login request DTO
/// </summary>
public record AppleLoginRequest(
    string IdentityToken,
    string? UserFirstName = null,
    string? UserLastName = null);