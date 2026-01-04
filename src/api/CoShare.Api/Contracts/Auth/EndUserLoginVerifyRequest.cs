namespace CoShare.Api.Contracts.Auth;

/// <summary>
/// Request to verify OTP and complete end-user login.
/// Maps to OpenAPI: EndUserLoginVerifyRequest (US-AUTH-002)
/// </summary>
public sealed record EndUserLoginVerifyRequest
{
    /// <summary>
    /// Phone number (Vietnam format, max 20 chars).
    /// </summary>
    public required string Phone { get; init; }

    /// <summary>
    /// 6-digit OTP code received via ZNS.
    /// </summary>
    public required string OtpCode { get; init; }

    /// <summary>
    /// Optional device information for token binding.
    /// </summary>
    public DeviceInfo? DeviceInfo { get; init; }
}
