using Microsoft.EntityFrameworkCore;
using VideGreniers.Application.Authentication.Models;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Infrastructure.Identity;

namespace VideGreniers.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for RefreshToken operations
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a refresh token by its token value
    /// </summary>
    public async Task<RefreshTokenInfo?> GetByTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
            return null;

        return new RefreshTokenInfo(
            refreshToken.Token,
            refreshToken.UserId.ToString(),
            refreshToken.ExpiresAt,
            refreshToken.IsRevoked,
            refreshToken.DeviceInfo);
    }

    /// <summary>
    /// Gets all active refresh tokens for a user
    /// </summary>
    public async Task<List<RefreshTokenInfo>> GetActiveTokensForUserAsync(string userId)
    {
        var guidUserId = Guid.Parse(userId);
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == guidUserId && rt.IsActive)
            .OrderBy(rt => rt.CreatedOnUtc)
            .ToListAsync();

        return tokens.Select(rt => new RefreshTokenInfo(
            rt.Token,
            rt.UserId.ToString(),
            rt.ExpiresAt,
            rt.IsRevoked,
            rt.DeviceInfo)).ToList();
    }

    /// <summary>
    /// Adds a new refresh token
    /// </summary>
    public async Task AddAsync(string token, string userId, DateTime expiresAt, string? deviceInfo)
    {
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = Guid.Parse(userId),
            ExpiresAt = expiresAt,
            DeviceInfo = deviceInfo,
            IsRevoked = false
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
    }

    /// <summary>
    /// Revokes a refresh token
    /// </summary>
    public async Task RevokeAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken != null && !refreshToken.IsRevoked)
        {
            refreshToken.Revoke("User logout");
            _context.RefreshTokens.Update(refreshToken);
        }
    }

    /// <summary>
    /// Revokes all refresh tokens for a user
    /// </summary>
    public async Task RevokeAllTokensForUserAsync(string userId, string reason)
    {
        var guidUserId = Guid.Parse(userId);
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == guidUserId && rt.IsActive)
            .ToListAsync();
        
        foreach (var token in activeTokens)
        {
            token.Revoke(reason);
        }

        _context.RefreshTokens.UpdateRange(activeTokens);
    }

    /// <summary>
    /// Removes expired refresh tokens
    /// </summary>
    public async Task RemoveExpiredTokensAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow || rt.IsRevoked)
            .Where(rt => rt.CreatedOnUtc <= DateTime.UtcNow.AddDays(-30)) // Keep revoked tokens for 30 days for audit
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
    }

    /// <summary>
    /// Saves changes to the database
    /// </summary>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}