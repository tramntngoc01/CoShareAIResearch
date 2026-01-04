namespace CoShare.Api.Contracts.Auth;

/// <summary>
/// Request to start end-user login by sending an OTP.
/// Maps to OpenAPI: EndUserLoginStartRequest (US-AUTH-002)
/// </summary>
public sealed record EndUserLoginStartRequest
{
    /// <summary>
    /// Phone number (Vietnam format, max 20 chars).
    /// </summary>
    public required string Phone { get; init; }
}
