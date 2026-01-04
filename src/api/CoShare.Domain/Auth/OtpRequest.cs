using CoShare.Domain.Common;

namespace CoShare.Domain.Auth;

/// <summary>
/// OTP request entity matching auth_otp_request table from DB-AUTH.
/// </summary>
public class OtpRequest : AuditableEntity
{
    public long Id { get; set; }

    /// <summary>
    /// Phone number (PII - must be masked in logs).
    /// </summary>
    public required string Phone { get; set; }

    public OtpPurpose Purpose { get; set; }

    /// <summary>
    /// Hash of the OTP code (never store raw OTP per Security-Baseline).
    /// </summary>
    public required string OtpCodeHash { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public OtpStatus Status { get; set; } = OtpStatus.Pending;

    public int AttemptCount { get; set; }

    public DateTimeOffset? LastAttemptAt { get; set; }

    public string? NotificationTemplateCode { get; set; }

    public string? NotificationMessageId { get; set; }

    public string? CorrelationId { get; set; }

    // Registration-specific data (stored temporarily until verification)
    public string? FullName { get; set; }
    public long? CompanyId { get; set; }

    /// <summary>
    /// Check if this OTP is still valid for verification.
    /// </summary>
    public bool IsValidForVerification(DateTimeOffset now) =>
        Status == OtpStatus.Pending && ExpiresAt > now && !IsDeleted;

    /// <summary>
    /// Mark as verified after successful OTP check.
    /// </summary>
    public void MarkVerified(DateTimeOffset now, string? updatedBy)
    {
        Status = OtpStatus.Verified;
        UpdatedAt = now;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Record a failed verification attempt.
    /// </summary>
    public void RecordFailedAttempt(DateTimeOffset now, string? updatedBy)
    {
        AttemptCount++;
        LastAttemptAt = now;
        UpdatedAt = now;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Cancel this OTP (e.g., when a new one is requested).
    /// </summary>
    public void Cancel(DateTimeOffset now, string? updatedBy)
    {
        Status = OtpStatus.Cancelled;
        UpdatedAt = now;
        UpdatedBy = updatedBy;
    }
}
