using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using VideGreniers.Application.Common.Interfaces;

namespace VideGreniers.Infrastructure.Identity.OAuth;

/// <summary>
/// Apple OAuth authentication service implementation
/// </summary>
public class AppleAuthService : IAppleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppleAuthService> _logger;
    private readonly HttpClient _httpClient;
    private static readonly Dictionary<string, RSA> _keyCache = new();

    public AppleAuthService(
        IConfiguration configuration,
        ILogger<AppleAuthService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Validates an Apple identity token and returns user information
    /// </summary>
    public async Task<AppleUserInfo?> ValidateIdentityTokenAsync(string identityToken)
    {
        try
        {
            var clientId = _configuration["OAuth:Apple:ClientId"];
            if (string.IsNullOrEmpty(clientId))
            {
                _logger.LogError("Apple OAuth ClientId is not configured");
                return null;
            }

            // Decode the JWT header to get the key ID
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(identityToken))
            {
                _logger.LogWarning("Apple token validation failed: Invalid JWT format");
                return null;
            }

            var jwt = handler.ReadJwtToken(identityToken);
            var kid = jwt.Header["kid"]?.ToString();
            
            if (string.IsNullOrEmpty(kid))
            {
                _logger.LogWarning("Apple token validation failed: Missing kid in header");
                return null;
            }

            // Get Apple's public key for this kid
            var publicKey = await GetApplePublicKeyAsync(kid);
            if (publicKey == null)
            {
                _logger.LogWarning("Apple token validation failed: Could not get public key for kid: {Kid}", kid);
                return null;
            }

            // Validate the token
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://appleid.apple.com",
                ValidateAudience = true,
                ValidAudience = clientId,
                ValidateLifetime = true,
                IssuerSigningKey = new RsaSecurityKey(publicKey),
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(2) // Allow 2 minutes clock skew
            };

            ClaimsPrincipal claimsPrincipal;
            try
            {
                claimsPrincipal = handler.ValidateToken(identityToken, validationParameters, out _);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Apple token validation failed: Invalid token");
                return null;
            }

            // Extract user information from claims
            var subject = claimsPrincipal.FindFirst("sub")?.Value;
            var email = claimsPrincipal.FindFirst("email")?.Value;
            var emailVerified = claimsPrincipal.FindFirst("email_verified")?.Value == "true";

            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Apple token validation failed: Missing required claims");
                return null;
            }

            var userInfo = new AppleUserInfo(
                Id: subject,
                Email: email,
                FirstName: null, // Apple doesn't provide name in JWT unless specifically requested
                LastName: null,
                EmailVerified: emailVerified);

            _logger.LogInformation("Apple token validated successfully for user: {Email}", email);
            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Apple token");
            return null;
        }
    }

    /// <summary>
    /// Gets Apple's public key for JWT signature validation
    /// </summary>
    private async Task<RSA?> GetApplePublicKeyAsync(string keyId)
    {
        try
        {
            // Check cache first
            if (_keyCache.TryGetValue(keyId, out var cachedKey))
            {
                return cachedKey;
            }

            // Fetch Apple's public keys
            const string appleKeysUrl = "https://appleid.apple.com/auth/keys";
            var response = await _httpClient.GetStringAsync(appleKeysUrl);
            
            using var document = JsonDocument.Parse(response);
            var keys = document.RootElement.GetProperty("keys");

            foreach (var key in keys.EnumerateArray())
            {
                var kid = key.GetProperty("kid").GetString();
                if (kid != keyId) continue;

                var kty = key.GetProperty("kty").GetString();
                if (kty != "RSA") continue;

                var n = key.GetProperty("n").GetString();
                var e = key.GetProperty("e").GetString();

                if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(e)) continue;

                var rsa = RSA.Create();
                rsa.ImportParameters(new RSAParameters
                {
                    Modulus = Base64UrlEncoder.DecodeBytes(n),
                    Exponent = Base64UrlEncoder.DecodeBytes(e)
                });

                // Cache the key
                _keyCache[keyId] = rsa;
                
                return rsa;
            }

            _logger.LogWarning("Apple public key not found for kid: {Kid}", keyId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Apple public key");
            return null;
        }
    }
}