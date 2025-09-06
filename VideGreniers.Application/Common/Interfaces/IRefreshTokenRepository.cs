using VideGreniers.Application.Authentication.Models;

namespace VideGreniers.Application.Common.Interfaces;

/// <summary>
/// Repository interface for RefreshToken operations
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Gets a refresh token by its token value
    /// </summary>
    /// <param name="token">The token value</param>
    /// <returns>RefreshToken if found, null otherwise</returns>
    Task<RefreshTokenInfo?> GetByTokenAsync(string token);

    /// <summary>
    /// Gets all active refresh tokens for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of active refresh tokens</returns>
    Task<List<RefreshTokenInfo>> GetActiveTokensForUserAsync(string userId);

    /// <summary>
    /// Adds a new refresh token
    /// </summary>
    /// <param name="token">Token string</param>
    /// <param name="userId">User ID</param>
    /// <param name="expiresAt">Expiration date</param>
    /// <param name="deviceInfo">Device information</param>
    Task AddAsync(string token, string userId, DateTime expiresAt, string? deviceInfo);

    /// <summary>
    /// Revokes a refresh token
    /// </summary>
    /// <param name="token">Token to revoke</param>
    Task RevokeAsync(string token);

    /// <summary>
    /// Revokes all refresh tokens for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="reason">Reason for revocation</param>
    Task RevokeAllTokensForUserAsync(string userId, string reason);

    /// <summary>
    /// Removes expired refresh tokens
    /// </summary>
    Task RemoveExpiredTokensAsync();

    /// <summary>
    /// Saves changes to the database
    /// </summary>
    Task<int> SaveChangesAsync();
}