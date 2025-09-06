using VideGreniers.Domain.Common.Models;

namespace VideGreniers.Infrastructure.Identity;

/// <summary>
/// Refresh token entity for managing token persistence
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// The actual refresh token value
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The user ID this refresh token belongs to
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// When this refresh token expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Optional device/client information
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Whether this token has been revoked
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// When this token was revoked (if revoked)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// The reason for revocation (optional)
    /// </summary>
    public string? RevocationReason { get; set; }

    /// <summary>
    /// If this token was replaced, the ID of the replacement token
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    /// <summary>
    /// Checks if this token is currently active (not expired and not revoked)
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired;

    /// <summary>
    /// Checks if this token has expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Revokes this refresh token
    /// </summary>
    /// <param name="reason">The reason for revocation</param>
    /// <param name="replacedByTokenId">Optional ID of the replacement token</param>
    public void Revoke(string? reason = null, Guid? replacedByTokenId = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevocationReason = reason;
        ReplacedByTokenId = replacedByTokenId;
    }
}