namespace CoShare.Domain.Auth;

/// <summary>
/// Status of an OTP request per DB-AUTH.
/// </summary>
public enum OtpStatus
{
    Pending,
    Verified,
    Expired,
    Cancelled
}
