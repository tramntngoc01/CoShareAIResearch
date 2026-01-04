namespace CoShare.Domain.Auth;

/// <summary>
/// AUTH module error codes per Error-Conventions and StoryPack US-AUTH-001, US-AUTH-002.
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
    
    // US-AUTH-002: Login errors
    public const string UserNotFound = "AUTH_USER_NOT_FOUND";
    public const string UserLocked = "AUTH_USER_LOCKED";
    
    public const string TokenInvalid = "AUTH_TOKEN_INVALID";
    public const string TokenExpired = "AUTH_TOKEN_EXPIRED";
    public const string RefreshTokenRevoked = "AUTH_REFRESH_TOKEN_REVOKED";
    public const string RefreshTokenExpired = "AUTH_REFRESH_TOKEN_EXPIRED";
    
    // US-AUTH-005: Admin login errors
    public const string AdminInvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string AdminLocked = "AUTH_ADMIN_LOCKED";
    public const string AdminNotAuthorized = "AUTH_ADMIN_NOT_AUTHORIZED";
}
