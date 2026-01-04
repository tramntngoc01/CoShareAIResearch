using CoShare.Domain.Common;

namespace CoShare.Domain.Auth;

/// <summary>
/// Refresh token entity matching auth_refresh_token table from DB-AUTH.
/// </summary>
public class RefreshToken : AuditableEntity
{
    public long Id { get; set; }

    /// <summary>
    /// End user ID (mutually exclusive with AdminUserId).
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// Admin user ID (mutually exclusive with UserId).
    /// </summary>
    public long? AdminUserId { get; set; }

    /// <summary>
    /// Hash of the refresh token (never store raw token).
    /// </summary>
    public required string TokenHash { get; set; }

    public string? DeviceId { get; set; }
    public string? UserAgent { get; set; }
    public string? Platform { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    public string? CorrelationId { get; set; }

    public bool IsValid(DateTimeOffset now) =>
        !IsDeleted && RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTimeOffset now, string reason, string? updatedBy)
    {
        RevokedAt = now;
        RevokedReason = reason;
        UpdatedAt = now;
        UpdatedBy = updatedBy;
    }
}
