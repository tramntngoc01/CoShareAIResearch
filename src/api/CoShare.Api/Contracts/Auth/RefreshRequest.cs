namespace CoShare.Api.Contracts.Auth;

/// <summary>
/// Request to refresh access token using a refresh token.
/// Implements US-AUTH-003: Auto-login via refresh token.
/// </summary>
public sealed record RefreshRequest
{
    /// <summary>
    /// The refresh token issued during login or registration.
    /// </summary>
    public required string RefreshToken { get; init; }
}
