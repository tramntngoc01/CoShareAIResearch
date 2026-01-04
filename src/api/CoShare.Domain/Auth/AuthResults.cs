namespace CoShare.Domain.Auth;

/// <summary>
/// Result of OTP request operation.
/// </summary>
public sealed record OtpRequestResult
{
    public bool Success { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static OtpRequestResult Ok() => new() { Success = true };

    public static OtpRequestResult Fail(string code, string message) =>
        new() { Success = false, ErrorCode = code, ErrorMessage = message };
}

/// <summary>
/// Result of OTP verification operation.
/// </summary>
public sealed record OtpVerifyResult
{
    public bool Success { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public OtpRequest? VerifiedOtp { get; init; }

    public static OtpVerifyResult Ok(OtpRequest otp) =>
        new() { Success = true, VerifiedOtp = otp };

    public static OtpVerifyResult Fail(string code, string message) =>
        new() { Success = false, ErrorCode = code, ErrorMessage = message };
}

/// <summary>
/// Result of user registration operation.
/// </summary>
public sealed record RegistrationResult
{
    public bool Success { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public long? UserId { get; init; }
    public string? FullName { get; init; }
    public string? Phone { get; init; }
    public long? CompanyId { get; init; }
    public int? Tier { get; init; }

    public static RegistrationResult Ok(long userId, string fullName, string phone, long companyId, int tier) =>
        new()
        {
            Success = true,
            UserId = userId,
            FullName = fullName,
            Phone = phone,
            CompanyId = companyId,
            Tier = tier
        };

    public static RegistrationResult Fail(string code, string message) =>
        new() { Success = false, ErrorCode = code, ErrorMessage = message };
}
