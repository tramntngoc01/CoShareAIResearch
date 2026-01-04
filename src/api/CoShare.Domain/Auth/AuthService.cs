using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoShare.Domain.Auth;

/// <summary>
/// Service interface for AUTH operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Request OTP for end-user registration (US-AUTH-001).
    /// </summary>
    Task<OtpRequestResult> RequestRegistrationOtpAsync(
        string phone,
        string fullName,
        long companyId,
        bool acceptTerms,
        string correlationId,
        CancellationToken ct = default);

    /// <summary>
    /// Verify OTP and complete end-user registration (US-AUTH-001).
    /// </summary>
    Task<RegistrationResult> VerifyRegistrationOtpAsync(
        string phone,
        string otpCode,
        string? deviceId,
        string? userAgent,
        string? platform,
        string correlationId,
        CancellationToken ct = default);
}

/// <summary>
/// AUTH service implementation with core business logic per US-AUTH-001.
/// </summary>
public partial class AuthService : IAuthService
{
    private readonly IOtpRequestRepository _otpRepo;
    private readonly IRefreshTokenRepository _tokenRepo;
    private readonly INotificationService _notifications;
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly AuthOptions _options;
    private readonly ILogger<AuthService> _logger;
    private readonly TimeProvider _timeProvider;

    // Vietnam phone regex: 10-11 digits starting with 0
    [GeneratedRegex(@"^0\d{9,10}$")]
    private static partial Regex VietnamPhoneRegex();

    private const string OtpTemplateCode = "OTP_REGISTER_V1";

    public AuthService(
        IOtpRequestRepository otpRepo,
        IRefreshTokenRepository tokenRepo,
        INotificationService notifications,
        IUserService userService,
        ITokenService tokenService,
        IOptions<AuthOptions> options,
        ILogger<AuthService> logger,
        TimeProvider timeProvider)
    {
        _otpRepo = otpRepo;
        _tokenRepo = tokenRepo;
        _notifications = notifications;
        _userService = userService;
        _tokenService = tokenService;
        _options = options.Value;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public async Task<OtpRequestResult> RequestRegistrationOtpAsync(
        string phone,
        string fullName,
        long companyId,
        bool acceptTerms,
        string correlationId,
        CancellationToken ct = default)
    {
        var now = _timeProvider.GetUtcNow();

        // AC: User must accept terms
        if (!acceptTerms)
        {
            _logger.LogWarning(
                "Registration OTP request rejected: terms not accepted. CorrelationId={CorrelationId}",
                correlationId);
            return OtpRequestResult.Fail(
                AuthErrorCodes.TermsNotAccepted,
                "Terms and conditions must be accepted.");
        }

        // EC1: Validate phone format
        if (!IsValidPhoneFormat(phone))
        {
            _logger.LogWarning(
                "Registration OTP request rejected: invalid phone format. CorrelationId={CorrelationId}",
                correlationId);
            return OtpRequestResult.Fail(
                AuthErrorCodes.InvalidPhoneFormat,
                "Phone number format is invalid.");
        }

        // AC: Check if phone already registered
        if (await _userService.PhoneExistsAsync(phone, ct))
        {
            _logger.LogWarning(
                "Registration OTP request rejected: phone already exists. CorrelationId={CorrelationId}",
                correlationId);
            return OtpRequestResult.Fail(
                AuthErrorCodes.UserAlreadyExists,
                "This phone number is already registered.");
        }

        // Validate company exists
        if (!await _userService.CompanyExistsAsync(companyId, ct))
        {
            _logger.LogWarning(
                "Registration OTP request rejected: company not found. CompanyId={CompanyId}, CorrelationId={CorrelationId}",
                companyId, correlationId);
            return OtpRequestResult.Fail(
                AuthErrorCodes.CompanyNotFound,
                "The specified company does not exist.");
        }

        // EC2: Rate limiting - check OTP requests in window
        var windowStart = now.AddSeconds(-_options.OtpRateLimitWindowSeconds);
        var recentCount = await _otpRepo.CountRecentRequestsAsync(
            phone, OtpPurpose.EndUserRegister, windowStart, ct);

        if (recentCount >= _options.OtpMaxRequestsPerWindow)
        {
            _logger.LogWarning(
                "Registration OTP request rate limited. Phone=***{PhoneSuffix}, Count={Count}, CorrelationId={CorrelationId}",
                MaskPhone(phone), recentCount, correlationId);
            return OtpRequestResult.Fail(
                AuthErrorCodes.OtpRateLimited,
                "Too many OTP requests. Please try again later.");
        }

        // Cancel any pending OTPs for this phone+purpose
        await _otpRepo.CancelPendingByPhoneAndPurposeAsync(
            phone, OtpPurpose.EndUserRegister, now, "system", ct);

        // Generate OTP
        var otpCode = OtpHelper.GenerateOtp(_options.OtpLength);
        var otpHash = OtpHelper.HashOtp(otpCode);

        // EC3: Send OTP via NOTIFICATIONS
        var messageId = await _notifications.SendOtpAsync(
            phone, otpCode, OtpTemplateCode, _options.OtpTtlSeconds, correlationId, ct);

        if (string.IsNullOrEmpty(messageId))
        {
            _logger.LogError(
                "OTP delivery failed via NOTIFICATIONS. CorrelationId={CorrelationId}",
                correlationId);
            return OtpRequestResult.Fail(
                AuthErrorCodes.OtpDeliveryFailed,
                "Failed to send OTP. Please try again.");
        }

        // Create OTP request record
        var otpRequest = new OtpRequest
        {
            Phone = phone,
            Purpose = OtpPurpose.EndUserRegister,
            OtpCodeHash = otpHash,
            ExpiresAt = now.AddSeconds(_options.OtpTtlSeconds),
            Status = OtpStatus.Pending,
            AttemptCount = 0,
            NotificationTemplateCode = OtpTemplateCode,
            NotificationMessageId = messageId,
            CorrelationId = correlationId,
            FullName = fullName,
            CompanyId = companyId,
            CreatedAt = now,
            CreatedBy = "system"
        };

        await _otpRepo.AddAsync(otpRequest, ct);
        await _otpRepo.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Registration OTP requested successfully. Phone=***{PhoneSuffix}, CorrelationId={CorrelationId}",
            MaskPhone(phone), correlationId);

        return OtpRequestResult.Ok();
    }

    /// <inheritdoc/>
    public async Task<RegistrationResult> VerifyRegistrationOtpAsync(
        string phone,
        string otpCode,
        string? deviceId,
        string? userAgent,
        string? platform,
        string correlationId,
        CancellationToken ct = default)
    {
        var now = _timeProvider.GetUtcNow();

        // Validate phone format
        if (!IsValidPhoneFormat(phone))
        {
            return RegistrationResult.Fail(
                AuthErrorCodes.InvalidPhoneFormat,
                "Phone number format is invalid.");
        }

        // Get pending OTP for this phone
        var otpRequest = await _otpRepo.GetPendingByPhoneAndPurposeAsync(
            phone, OtpPurpose.EndUserRegister, ct);

        // AC3: Check if OTP exists and is valid
        if (otpRequest is null || !otpRequest.IsValidForVerification(now))
        {
            _logger.LogWarning(
                "OTP verification failed: no valid pending OTP. Phone=***{PhoneSuffix}, CorrelationId={CorrelationId}",
                MaskPhone(phone), correlationId);
            return RegistrationResult.Fail(
                AuthErrorCodes.OtpInvalidOrExpired,
                "OTP is invalid or expired.");
        }

        // Check max attempts
        if (otpRequest.AttemptCount >= _options.OtpMaxVerificationAttempts)
        {
            _logger.LogWarning(
                "OTP verification failed: max attempts exceeded. Phone=***{PhoneSuffix}, CorrelationId={CorrelationId}",
                MaskPhone(phone), correlationId);
            return RegistrationResult.Fail(
                AuthErrorCodes.OtpMaxAttemptsExceeded,
                "Maximum verification attempts exceeded. Please request a new OTP.");
        }

        // Verify OTP code
        if (!OtpHelper.VerifyOtp(otpCode, otpRequest.OtpCodeHash))
        {
            // Record failed attempt
            otpRequest.RecordFailedAttempt(now, "system");
            await _otpRepo.UpdateAsync(otpRequest, ct);
            await _otpRepo.SaveChangesAsync(ct);

            _logger.LogWarning(
                "OTP verification failed: incorrect code. Phone=***{PhoneSuffix}, Attempts={Attempts}, CorrelationId={CorrelationId}",
                MaskPhone(phone), otpRequest.AttemptCount, correlationId);

            return RegistrationResult.Fail(
                AuthErrorCodes.OtpInvalidOrExpired,
                "OTP is invalid or expired.");
        }

        // EC4: Verify phone in OTP matches request phone
        if (!string.Equals(otpRequest.Phone, phone, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "OTP verification failed: phone mismatch. CorrelationId={CorrelationId}",
                correlationId);
            return RegistrationResult.Fail(
                AuthErrorCodes.OtpInvalidOrExpired,
                "OTP is invalid or expired.");
        }

        // Check if user was created in the meantime
        if (await _userService.PhoneExistsAsync(phone, ct))
        {
            _logger.LogWarning(
                "OTP verification failed: user already exists. Phone=***{PhoneSuffix}, CorrelationId={CorrelationId}",
                MaskPhone(phone), correlationId);
            return RegistrationResult.Fail(
                AuthErrorCodes.UserAlreadyExists,
                "This phone number is already registered.");
        }

        // Mark OTP as verified
        otpRequest.MarkVerified(now, "system");
        await _otpRepo.UpdateAsync(otpRequest, ct);

        // AC2: Create the end user
        var userId = await _userService.CreateEndUserAsync(
            phone,
            otpRequest.FullName!,
            otpRequest.CompanyId!.Value,
            correlationId,
            ct);

        await _otpRepo.SaveChangesAsync(ct);

        _logger.LogInformation(
            "End user registered successfully. UserId={UserId}, Phone=***{PhoneSuffix}, CorrelationId={CorrelationId}",
            userId, MaskPhone(phone), correlationId);

        // Return registration result (tokens will be generated by controller)
        return RegistrationResult.Ok(
            userId,
            otpRequest.FullName!,
            phone,
            otpRequest.CompanyId!.Value,
            tier: 3); // New end users are Tier 3
    }

    /// <summary>
    /// Validate phone number format (Vietnam).
    /// </summary>
    private static bool IsValidPhoneFormat(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        return VietnamPhoneRegex().IsMatch(phone);
    }

    /// <summary>
    /// Mask phone number for logging (show last 4 digits only).
    /// </summary>
    private static string MaskPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 4)
            return "****";
        return phone[^4..];
    }
}
