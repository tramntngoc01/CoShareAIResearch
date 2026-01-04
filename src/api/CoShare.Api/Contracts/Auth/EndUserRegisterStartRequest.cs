namespace CoShare.Api.Contracts.Auth;

/// <summary>
/// Request to start end-user registration by sending an OTP.
/// Maps to OpenAPI: EndUserRegisterStartRequest
/// </summary>
public sealed record EndUserRegisterStartRequest
{
    /// <summary>
    /// Full name of the user.
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// Company ID the user belongs to.
    /// </summary>
    public required long CompanyId { get; init; }

    /// <summary>
    /// Phone number (Vietnam format, max 20 chars).
    /// </summary>
    public required string Phone { get; init; }

    /// <summary>
    /// User must accept terms and conditions.
    /// </summary>
    public required bool AcceptTerms { get; init; }
}
