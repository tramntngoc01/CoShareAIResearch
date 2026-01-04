namespace CoShare.Api.Contracts.Auth;

/// <summary>
/// Request to verify OTP and complete end-user registration.
/// Maps to OpenAPI: EndUserRegisterVerifyRequest
/// </summary>
public sealed record EndUserRegisterVerifyRequest
{
    /// <summary>
    /// Phone number that received the OTP.
    /// </summary>
    public required string Phone { get; init; }

    /// <summary>
    /// The OTP code entered by the user.
    /// </summary>
    public required string OtpCode { get; init; }

    /// <summary>
    /// Optional device information for token binding.
    /// </summary>
    public DeviceInfo? DeviceInfo { get; init; }
}
