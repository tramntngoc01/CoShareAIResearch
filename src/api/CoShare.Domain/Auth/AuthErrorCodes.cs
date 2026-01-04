namespace CoShare.Domain.Auth;

/// <summary>
/// AUTH module error codes per Error-Conventions and StoryPack US-AUTH-001.
/// </summary>
public static class AuthErrorCodes
{
    public const string InvalidPhoneFormat = "AUTH_INVALID_PHONE_FORMAT";
    public const string OtpRateLimited = "AUTH_OTP_RATE_LIMITED";
    public const string OtpDeliveryFailed = "AUTH_OTP_DELIVERY_FAILED";
    public const string OtpInvalidOrExpired = "AUTH_OTP_INVALID_OR_EXPIRED";
    public const string UserAlreadyExists = "AUTH_USER_ALREADY_EXISTS";
    public const string IdempotencyConflict = "AUTH_IDEMPOTENCY_CONFLICT";
    public const string TermsNotAccepted = "AUTH_TERMS_NOT_ACCEPTED";
    public const string CompanyNotFound = "AUTH_COMPANY_NOT_FOUND";
    public const string OtpMaxAttemptsExceeded = "AUTH_OTP_MAX_ATTEMPTS_EXCEEDED";
}
