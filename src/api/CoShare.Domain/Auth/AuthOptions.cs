namespace CoShare.Domain.Auth;

/// <summary>
/// AUTH configuration options.
/// </summary>
public class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>
    /// OTP time-to-live in seconds (default: 120 = 2 minutes).
    /// </summary>
    public int OtpTtlSeconds { get; set; } = 120;

    /// <summary>
    /// Maximum OTP requests per phone within the rate limit window.
    /// </summary>
    public int OtpMaxRequestsPerWindow { get; set; } = 3;

    /// <summary>
    /// Rate limit window in seconds (default: 300 = 5 minutes).
    /// </summary>
    public int OtpRateLimitWindowSeconds { get; set; } = 300;

    /// <summary>
    /// Maximum verification attempts per OTP before invalidation.
    /// </summary>
    public int OtpMaxVerificationAttempts { get; set; } = 5;

    /// <summary>
    /// JWT access token expiry in seconds.
    /// </summary>
    public int AccessTokenExpirySeconds { get; set; } = 3600;

    /// <summary>
    /// Refresh token expiry in days.
    /// </summary>
    public int RefreshTokenExpiryDays { get; set; } = 30;

    /// <summary>
    /// OTP length (default: 6 digits).
    /// </summary>
    public int OtpLength { get; set; } = 6;

    /// <summary>
    /// JWT signing key (must be set in configuration).
    /// </summary>
    public string JwtSigningKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT issuer.
    /// </summary>
    public string JwtIssuer { get; set; } = "CoShare";

    /// <summary>
    /// JWT audience.
    /// </summary>
    public string JwtAudience { get; set; } = "CoShare";
}
