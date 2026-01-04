using CoShare.Domain.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CoShare.Domain.UnitTests.Auth;

/// <summary>
/// Unit tests for AUTH service - US-AUTH-001 business rules.
/// Test ID: UT-AUTH-001
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IOtpRequestRepository> _otpRepoMock;
    private readonly Mock<IRefreshTokenRepository> _tokenRepoMock;
    private readonly Mock<INotificationService> _notificationsMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly AuthOptions _options;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _otpRepoMock = new Mock<IOtpRequestRepository>();
        _tokenRepoMock = new Mock<IRefreshTokenRepository>();
        _notificationsMock = new Mock<INotificationService>();
        _userServiceMock = new Mock<IUserService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _timeProviderMock = new Mock<TimeProvider>();

        _options = new AuthOptions
        {
            OtpTtlSeconds = 120,
            OtpMaxRequestsPerWindow = 3,
            OtpRateLimitWindowSeconds = 300,
            OtpMaxVerificationAttempts = 5,
            OtpLength = 6
        };

        var optionsMock = new Mock<IOptions<AuthOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);

        _timeProviderMock.Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(2026, 1, 4, 10, 0, 0, TimeSpan.Zero));

        _authService = new AuthService(
            _otpRepoMock.Object,
            _tokenRepoMock.Object,
            _notificationsMock.Object,
            _userServiceMock.Object,
            _tokenServiceMock.Object,
            optionsMock.Object,
            _loggerMock.Object,
            _timeProviderMock.Object);
    }

    #region RequestRegistrationOtpAsync Tests

    [Fact]
    public async Task RequestRegistrationOtp_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        _userServiceMock.Setup(x => x.PhoneExistsAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userServiceMock.Setup(x => x.CompanyExistsAsync(501, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _otpRepoMock.Setup(x => x.CountRecentRequestsAsync(
                "0912345678", OtpPurpose.EndUserRegister, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _notificationsMock.Setup(x => x.SendOtpAsync(
                "0912345678", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("msg-123");

        // Act
        var result = await _authService.RequestRegistrationOtpAsync(
            "0912345678", "Trần Thị B", 501, acceptTerms: true, "corr-1");

        // Assert
        Assert.True(result.Success);
        _otpRepoMock.Verify(x => x.AddAsync(It.IsAny<OtpRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _otpRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestRegistrationOtp_WithTermsNotAccepted_ReturnsError()
    {
        // Act
        var result = await _authService.RequestRegistrationOtpAsync(
            "0912345678", "Test User", 501, acceptTerms: false, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.TermsNotAccepted, result.ErrorCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("091234567")] // 9 digits after 0
    [InlineData("091234567890")] // 12 digits
    [InlineData("abc12345678")]
    [InlineData("1912345678")] // Doesn't start with 0
    public async Task RequestRegistrationOtp_WithInvalidPhoneFormat_ReturnsError(string phone)
    {
        // Act
        var result = await _authService.RequestRegistrationOtpAsync(
            phone, "Test User", 501, acceptTerms: true, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.InvalidPhoneFormat, result.ErrorCode);
    }

    [Fact]
    public async Task RequestRegistrationOtp_WithExistingPhone_ReturnsUserAlreadyExists()
    {
        // Arrange
        _userServiceMock.Setup(x => x.PhoneExistsAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RequestRegistrationOtpAsync(
            "0912345678", "Test User", 501, acceptTerms: true, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.UserAlreadyExists, result.ErrorCode);
    }

    [Fact]
    public async Task RequestRegistrationOtp_WithInvalidCompany_ReturnsError()
    {
        // Arrange
        _userServiceMock.Setup(x => x.PhoneExistsAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userServiceMock.Setup(x => x.CompanyExistsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.RequestRegistrationOtpAsync(
            "0912345678", "Test User", 999, acceptTerms: true, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.CompanyNotFound, result.ErrorCode);
    }

    [Fact]
    public async Task RequestRegistrationOtp_WhenRateLimited_ReturnsRateLimitError()
    {
        // Arrange
        _userServiceMock.Setup(x => x.PhoneExistsAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userServiceMock.Setup(x => x.CompanyExistsAsync(501, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _otpRepoMock.Setup(x => x.CountRecentRequestsAsync(
                "0912345678", OtpPurpose.EndUserRegister, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3); // Max is 3

        // Act
        var result = await _authService.RequestRegistrationOtpAsync(
            "0912345678", "Test User", 501, acceptTerms: true, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.OtpRateLimited, result.ErrorCode);
    }

    [Fact]
    public async Task RequestRegistrationOtp_WhenNotificationFails_ReturnsDeliveryError()
    {
        // Arrange
        _userServiceMock.Setup(x => x.PhoneExistsAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userServiceMock.Setup(x => x.CompanyExistsAsync(501, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _otpRepoMock.Setup(x => x.CountRecentRequestsAsync(
                "0912345678", OtpPurpose.EndUserRegister, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _notificationsMock.Setup(x => x.SendOtpAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null); // Delivery failed

        // Act
        var result = await _authService.RequestRegistrationOtpAsync(
            "0912345678", "Test User", 501, acceptTerms: true, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.OtpDeliveryFailed, result.ErrorCode);
    }

    #endregion

    #region VerifyRegistrationOtpAsync Tests

    [Fact]
    public async Task VerifyRegistrationOtp_WithValidOtp_ReturnsSuccess()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 1, 4, 10, 0, 0, TimeSpan.Zero);
        var otpRequest = new OtpRequest
        {
            Id = 1,
            Phone = "0912345678",
            Purpose = OtpPurpose.EndUserRegister,
            OtpCodeHash = OtpHelper.HashOtp("123456"),
            ExpiresAt = now.AddMinutes(2),
            Status = OtpStatus.Pending,
            AttemptCount = 0,
            FullName = "Trần Thị B",
            CompanyId = 501,
            CreatedAt = now.AddMinutes(-1)
        };

        _otpRepoMock.Setup(x => x.GetPendingByPhoneAndPurposeAsync(
                "0912345678", OtpPurpose.EndUserRegister, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);
        _userServiceMock.Setup(x => x.PhoneExistsAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userServiceMock.Setup(x => x.CreateEndUserAsync(
                "0912345678", "Trần Thị B", 501, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10001);

        // Act
        var result = await _authService.VerifyRegistrationOtpAsync(
            "0912345678", "123456", null, null, null, "corr-1");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(10001, result.UserId);
        Assert.Equal("Trần Thị B", result.FullName);
        Assert.Equal("0912345678", result.Phone);
        Assert.Equal(501, result.CompanyId);
        Assert.Equal(3, result.Tier);
    }

    [Fact]
    public async Task VerifyRegistrationOtp_WithExpiredOtp_ReturnsError()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 1, 4, 10, 0, 0, TimeSpan.Zero);
        var otpRequest = new OtpRequest
        {
            Id = 1,
            Phone = "0912345678",
            Purpose = OtpPurpose.EndUserRegister,
            OtpCodeHash = OtpHelper.HashOtp("123456"),
            ExpiresAt = now.AddMinutes(-1), // Expired
            Status = OtpStatus.Pending,
            AttemptCount = 0,
            FullName = "Test User",
            CompanyId = 501,
            CreatedAt = now.AddMinutes(-3)
        };

        _otpRepoMock.Setup(x => x.GetPendingByPhoneAndPurposeAsync(
                "0912345678", OtpPurpose.EndUserRegister, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);

        // Act
        var result = await _authService.VerifyRegistrationOtpAsync(
            "0912345678", "123456", null, null, null, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.OtpInvalidOrExpired, result.ErrorCode);
    }

    [Fact]
    public async Task VerifyRegistrationOtp_WithWrongOtpCode_RecordsAttemptAndReturnsError()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 1, 4, 10, 0, 0, TimeSpan.Zero);
        var otpRequest = new OtpRequest
        {
            Id = 1,
            Phone = "0912345678",
            Purpose = OtpPurpose.EndUserRegister,
            OtpCodeHash = OtpHelper.HashOtp("123456"),
            ExpiresAt = now.AddMinutes(2),
            Status = OtpStatus.Pending,
            AttemptCount = 0,
            FullName = "Test User",
            CompanyId = 501,
            CreatedAt = now.AddMinutes(-1)
        };

        _otpRepoMock.Setup(x => x.GetPendingByPhoneAndPurposeAsync(
                "0912345678", OtpPurpose.EndUserRegister, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);

        // Act
        var result = await _authService.VerifyRegistrationOtpAsync(
            "0912345678", "999999", null, null, null, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.OtpInvalidOrExpired, result.ErrorCode);
        Assert.Equal(1, otpRequest.AttemptCount); // Attempt was recorded
        _otpRepoMock.Verify(x => x.UpdateAsync(otpRequest, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyRegistrationOtp_WithMaxAttemptsExceeded_ReturnsError()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 1, 4, 10, 0, 0, TimeSpan.Zero);
        var otpRequest = new OtpRequest
        {
            Id = 1,
            Phone = "0912345678",
            Purpose = OtpPurpose.EndUserRegister,
            OtpCodeHash = OtpHelper.HashOtp("123456"),
            ExpiresAt = now.AddMinutes(2),
            Status = OtpStatus.Pending,
            AttemptCount = 5, // Max attempts already reached
            FullName = "Test User",
            CompanyId = 501,
            CreatedAt = now.AddMinutes(-1)
        };

        _otpRepoMock.Setup(x => x.GetPendingByPhoneAndPurposeAsync(
                "0912345678", OtpPurpose.EndUserRegister, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);

        // Act
        var result = await _authService.VerifyRegistrationOtpAsync(
            "0912345678", "123456", null, null, null, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.OtpMaxAttemptsExceeded, result.ErrorCode);
    }

    [Fact]
    public async Task VerifyRegistrationOtp_WithNoPendingOtp_ReturnsError()
    {
        // Arrange
        _otpRepoMock.Setup(x => x.GetPendingByPhoneAndPurposeAsync(
                "0912345678", OtpPurpose.EndUserRegister, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OtpRequest?)null);

        // Act
        var result = await _authService.VerifyRegistrationOtpAsync(
            "0912345678", "123456", null, null, null, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.OtpInvalidOrExpired, result.ErrorCode);
    }

    [Fact]
    public async Task VerifyRegistrationOtp_WhenUserCreatedInMeantime_ReturnsError()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 1, 4, 10, 0, 0, TimeSpan.Zero);
        var otpRequest = new OtpRequest
        {
            Id = 1,
            Phone = "0912345678",
            Purpose = OtpPurpose.EndUserRegister,
            OtpCodeHash = OtpHelper.HashOtp("123456"),
            ExpiresAt = now.AddMinutes(2),
            Status = OtpStatus.Pending,
            AttemptCount = 0,
            FullName = "Test User",
            CompanyId = 501,
            CreatedAt = now.AddMinutes(-1)
        };

        _otpRepoMock.Setup(x => x.GetPendingByPhoneAndPurposeAsync(
                "0912345678", OtpPurpose.EndUserRegister, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);
        _userServiceMock.Setup(x => x.PhoneExistsAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // User was created in meantime

        // Act
        var result = await _authService.VerifyRegistrationOtpAsync(
            "0912345678", "123456", null, null, null, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.UserAlreadyExists, result.ErrorCode);
    }

    #endregion
}
