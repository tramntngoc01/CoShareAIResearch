namespace CoShare.Domain.Auth;

/// <summary>
/// Interface for NOTIFICATIONS module integration (OTP delivery via ZNS).
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send an OTP via ZNS to the given phone number.
    /// </summary>
    /// <param name="phone">Target phone number.</param>
    /// <param name="otpCode">The OTP code to send.</param>
    /// <param name="templateCode">ZNS template code.</param>
    /// <param name="ttlSeconds">TTL for the OTP.</param>
    /// <param name="correlationId">Correlation ID for tracing.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Message ID if successful, null if failed.</returns>
    Task<string?> SendOtpAsync(
        string phone,
        string otpCode,
        string templateCode,
        int ttlSeconds,
        string correlationId,
        CancellationToken ct = default);
}
