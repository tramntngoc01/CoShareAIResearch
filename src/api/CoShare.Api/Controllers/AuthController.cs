using CoShare.Api.Contracts.Auth;
using CoShare.Api.Contracts.Common;
using CoShare.Domain.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CoShare.Api.Controllers;

/// <summary>
/// AUTH controller for end-user registration and login.
/// Implements US-AUTH-001: End User registration via SĐT + OTP ZNS.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly AuthOptions _authOptions;
    private readonly ILogger<AuthController> _logger;
    private readonly TimeProvider _timeProvider;

    public AuthController(
        IAuthService authService,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepo,
        Microsoft.Extensions.Options.IOptions<AuthOptions> authOptions,
        ILogger<AuthController> logger,
        TimeProvider timeProvider)
    {
        _authService = authService;
        _tokenService = tokenService;
        _refreshTokenRepo = refreshTokenRepo;
        _authOptions = authOptions.Value;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Admin login (Admin Portal) - US-AUTH-005.
    /// POST /api/v1/auth/login
    /// </summary>
    /// <remarks>
    /// Authenticates an admin user by username (email/phone) and password.
    /// AC1: Admin with valid credentials can login and see dashboard for their role.
    /// AC2: End Users cannot login to Admin Portal even with valid credentials.
    /// AC4: Generic error messages do not reveal account details.
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AdminLogin(
        [FromBody] AdminLoginRequest request,
        [FromHeader(Name = "X-Correlation-Id")] string? correlationId,
        CancellationToken ct)
    {
        correlationId ??= Guid.NewGuid().ToString();
        Response.Headers["X-Correlation-Id"] = correlationId;

        var result = await _authService.AdminLoginAsync(request.Username, request.Password, correlationId, ct);

        if (!result.Success)
        {
            var statusCode = MapErrorCodeToStatusCode(result.ErrorCode!);
            return StatusCode(statusCode, CreateErrorResponse(result.ErrorCode!, result.ErrorMessage!, correlationId));
        }

        // AC3: Log successful login (correlationId included)
        _logger.LogInformation(
            "Admin login successful, tokens issued. CorrelationId={CorrelationId}",
            correlationId);

        var response = new AdminLoginResponse
        {
            AccessToken = result.AccessToken!,
            RefreshToken = result.RefreshToken!,
            ExpiresIn = result.ExpiresIn,
            TokenType = "Bearer"
        };

        return Ok(response);
    }

    /// <summary>
    /// Start end-user registration by requesting an OTP.
    /// POST /api/v1/auth/end-user/register/request-otp
    /// </summary>
    /// <remarks>
    /// Validates input and sends an OTP via NOTIFICATIONS (ZNS).
    /// Rate limited per phone number.
    /// </remarks>
    [HttpPost("end-user/register/request-otp")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RequestRegistrationOtp(
        [FromBody] EndUserRegisterStartRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromHeader(Name = "X-Correlation-Id")] string? correlationId,
        CancellationToken ct)
    {
        correlationId ??= Guid.NewGuid().ToString();
        Response.Headers["X-Correlation-Id"] = correlationId;

        // TODO: Implement idempotency check with idempotencyKey

        var result = await _authService.RequestRegistrationOtpAsync(
            request.Phone,
            request.FullName,
            request.CompanyId,
            request.AcceptTerms,
            correlationId,
            ct);

        if (!result.Success)
        {
            var statusCode = MapErrorCodeToStatusCode(result.ErrorCode!);
            return StatusCode(statusCode, CreateErrorResponse(result.ErrorCode!, result.ErrorMessage!, correlationId));
        }

        return Accepted();
    }

    /// <summary>
    /// Verify OTP and complete end-user registration.
    /// POST /api/v1/auth/end-user/register/verify-otp
    /// </summary>
    /// <remarks>
    /// Verifies the OTP for the given phone number and, on success,
    /// creates the end-user account and issues tokens.
    /// </remarks>
    [HttpPost("end-user/register/verify-otp")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> VerifyRegistrationOtp(
        [FromBody] EndUserRegisterVerifyRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromHeader(Name = "X-Correlation-Id")] string? correlationId,
        CancellationToken ct)
    {
        correlationId ??= Guid.NewGuid().ToString();
        Response.Headers["X-Correlation-Id"] = correlationId;

        // TODO: Implement idempotency check with idempotencyKey

        var result = await _authService.VerifyRegistrationOtpAsync(
            request.Phone,
            request.OtpCode,
            request.DeviceInfo?.DeviceId,
            request.DeviceInfo?.UserAgent,
            request.DeviceInfo?.Platform,
            correlationId,
            ct);

        if (!result.Success)
        {
            var statusCode = MapErrorCodeToStatusCode(result.ErrorCode!);
            return StatusCode(statusCode, CreateErrorResponse(result.ErrorCode!, result.ErrorMessage!, correlationId));
        }

        // Generate tokens
        var (accessToken, expiresIn) = _tokenService.GenerateAccessToken(
            result.UserId!.Value,
            result.Phone!,
            result.Tier!.Value,
            result.CompanyId!.Value);

        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashRefreshToken(refreshTokenValue);

        // Store refresh token
        var now = _timeProvider.GetUtcNow();
        var refreshToken = new RefreshToken
        {
            UserId = result.UserId,
            TokenHash = refreshTokenHash,
            DeviceId = request.DeviceInfo?.DeviceId,
            UserAgent = request.DeviceInfo?.UserAgent,
            Platform = request.DeviceInfo?.Platform,
            ExpiresAt = now.AddDays(_authOptions.RefreshTokenExpiryDays),
            CorrelationId = correlationId,
            CreatedAt = now,
            CreatedBy = "system"
        };

        await _refreshTokenRepo.AddAsync(refreshToken, ct);
        await _refreshTokenRepo.SaveChangesAsync(ct);

        _logger.LogInformation(
            "End user registered and tokens issued. UserId={UserId}, CorrelationId={CorrelationId}",
            result.UserId, correlationId);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresIn = expiresIn,
            TokenType = "Bearer",
            User = new EndUserProfileSummary
            {
                UserId = result.UserId!.Value,
                FullName = result.FullName!,
                Phone = result.Phone!,
                CompanyId = result.CompanyId!.Value,
                Tier = result.Tier!.Value
            },
            Roles = new List<string> { "EndUser" }
        };

        return Ok(response);
    }

    private static int MapErrorCodeToStatusCode(string errorCode)
    {
        return errorCode switch
        {
            AuthErrorCodes.InvalidPhoneFormat => StatusCodes.Status400BadRequest,
            AuthErrorCodes.TermsNotAccepted => StatusCodes.Status400BadRequest,
            AuthErrorCodes.CompanyNotFound => StatusCodes.Status400BadRequest,
            AuthErrorCodes.OtpRateLimited => StatusCodes.Status429TooManyRequests,
            AuthErrorCodes.OtpDeliveryFailed => StatusCodes.Status500InternalServerError,
            AuthErrorCodes.OtpInvalidOrExpired => StatusCodes.Status401Unauthorized,
            AuthErrorCodes.OtpMaxAttemptsExceeded => StatusCodes.Status401Unauthorized,
            AuthErrorCodes.UserAlreadyExists => StatusCodes.Status409Conflict,
            AuthErrorCodes.IdempotencyConflict => StatusCodes.Status409Conflict,
            // US-AUTH-005: Admin login errors
            AuthErrorCodes.AdminInvalidCredentials => StatusCodes.Status401Unauthorized,
            AuthErrorCodes.AdminLocked => StatusCodes.Status403Forbidden,
            AuthErrorCodes.AdminNotAuthorized => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static ErrorResponse CreateErrorResponse(string code, string message, string correlationId)
    {
        return new ErrorResponse
        {
            Error = new ErrorDetail
            {
                Code = code,
                Message = message,
                CorrelationId = correlationId
            }
        };
    }
}
