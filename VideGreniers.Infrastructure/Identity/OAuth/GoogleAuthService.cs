using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VideGreniers.Application.Common.Interfaces;

namespace VideGreniers.Infrastructure.Identity.OAuth;

/// <summary>
/// Google OAuth authentication service implementation
/// </summary>
public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Validates a Google ID token and returns user information
    /// </summary>
    public async Task<GoogleUserInfo?> ValidateIdTokenAsync(string idToken)
    {
        try
        {
            var clientId = _configuration["OAuth:Google:ClientId"];
            if (string.IsNullOrEmpty(clientId))
            {
                _logger.LogError("Google OAuth ClientId is not configured");
                return null;
            }

            // Validate the Google ID token
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });

            if (payload == null)
            {
                _logger.LogWarning("Google token validation failed: Invalid token");
                return null;
            }

            // Extract user information
            var userInfo = new GoogleUserInfo(
                Id: payload.Subject,
                Email: payload.Email,
                Name: payload.Name,
                FirstName: payload.GivenName,
                LastName: payload.FamilyName,
                Picture: payload.Picture,
                EmailVerified: payload.EmailVerified
            );

            _logger.LogInformation("Google token validated successfully for user: {Email}", payload.Email);
            return userInfo;
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Google token validation failed: Invalid JWT");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google token");
            return null;
        }
    }
}