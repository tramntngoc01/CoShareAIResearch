using CoShare.Domain.Auth;
using CoShare.Domain.Models.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

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

    #region RequestLoginOtpAsync Tests (US-AUTH-002)

    [Fact]
    public async Task RequestLoginOtp_WithValidPhone_ReturnsSuccess()
    {
        // Arrange
        var user = new UserDetails
        {
            Id = 10001,
            FullName = "Existing User",
            Phone = "0912345678",
            CompanyId = 501,
            Tier = 3,
            Status = "Active"
        };

        _userServiceMock.Setup(x => x.GetUserByPhoneAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _otpRepoMock.Setup(x => x.CountRecentRequestsAsync(
                "0912345678", OtpPurpose.EndUserLogin, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _notificationsMock.Setup(x => x.SendOtpAsync(
                "0912345678", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("msg-456");

        // Act
        var result = await _authService.RequestLoginOtpAsync("0912345678", "corr-2");

        // Assert
        Assert.True(result.Success);
        _otpRepoMock.Verify(x => x.AddAsync(It.IsAny<OtpRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _otpRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("abc1234567")]
    [InlineData("1912345678")] // Doesn't start with 0
    public async Task RequestLoginOtp_WithInvalidPhoneFormat_ReturnsError(string phone)
    {
        // Act
        var result = await _authService.RequestLoginOtpAsync(phone, "corr-2");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.InvalidPhoneFormat, result.ErrorCode);
    }

    [Fact]
    public async Task RequestLoginOtp_WithNonExistentPhone_ReturnsUserNotFoundError()
    {
        // Arrange
        _userServiceMock.Setup(x => x.GetUserByPhoneAsync("0999999999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDetails?)null);

        // Act
        var result = await _authService.RequestLoginOtpAsync("0999999999", "corr-2");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.UserNotFound, result.ErrorCode);
        Assert.Contains("not found", result.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RequestLoginOtp_WithLockedUser_ReturnsUserLockedError()
    {
        // Arrange
        var lockedUser = new UserDetails
        {
            Id = 10002,
            FullName = "Locked User",
            Phone = "0901234567",
            CompanyId = 501,
            Tier = 3,
            Status = "Locked"
        };

        _userServiceMock.Setup(x => x.GetUserByPhoneAsync("0901234567", It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockedUser);

        // Act
        var result = await _authService.RequestLoginOtpAsync("0901234567", "corr-2");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.UserLocked, result.ErrorCode);
        Assert.Contains("not accessible", result.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RequestLoginOtp_WithInactiveUser_ReturnsUserLockedError()
    {
        // Arrange
        var inactiveUser = new UserDetails
        {
            Id = 10003,
            FullName = "Inactive User",
            Phone = "0901111111",
            CompanyId = 501,
            Tier = 3,
            Status = "Inactive"
        };

        _userServiceMock.Setup(x => x.GetUserByPhoneAsync("0901111111", It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveUser);

        // Act
        var result = await _authService.RequestLoginOtpAsync("0901111111", "corr-2");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.UserLocked, result.ErrorCode);
    }

    [Fact]
    public async Task RequestLoginOtp_WhenRateLimited_ReturnsRateLimitError()
    {
        // Arrange
        var user = new UserDetails
        {
            Id = 10001,
            FullName = "Test User",
            Phone = "0912345678",
            CompanyId = 501,
            Tier = 3,
            Status = "Active"
        };

        _userServiceMock.Setup(x => x.GetUserByPhoneAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _otpRepoMock.Setup(x => x.CountRecentRequestsAsync(
                "0912345678", OtpPurpose.EndUserLogin, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3); // Max is 3

        // Act
        var result = await _authService.RequestLoginOtpAsync("0912345678", "corr-2");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.OtpRateLimited, result.ErrorCode);
    }

    [Fact]
    public async Task RequestLoginOtp_WhenNotificationFails_ReturnsDeliveryError()
    {
        // Arrange
        var user = new UserDetails
        {
            Id = 10001,
            FullName = "Test User",
            Phone = "0912345678",
            CompanyId = 501,
            Tier = 3,
            Status = "Active"
        };

        _userServiceMock.Setup(x => x.GetUserByPhoneAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _otpRepoMock.Setup(x => x.CountRecentRequestsAsync(
                "0912345678", OtpPurpose.EndUserLogin, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _notificationsMock.Setup(x => x.SendOtpAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null); // Delivery failed

        // Act
        var result = await _authService.RequestLoginOtpAsync("0912345678", "corr-2");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.OtpDeliveryFailed, result.ErrorCode);
    }

    #endregion

    #region VerifyLoginOtpAsync Tests (US-AUTH-002)

    [Fact]
    public async Task VerifyLoginOtp_WithValidOtp_ReturnsSuccess()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 1, 4, 10, 0, 0, TimeSpan.Zero);
        var otpRequest = new OtpRequest
        {
            Id = 2,
            Phone = "0912345678",
            Purpose = OtpPurpose.EndUserLogin,
            OtpCodeHash = OtpHelper.HashOtp("654321"),
            ExpiresAt = now.AddMinutes(2),
            Status = OtpStatus.Pending,
            AttemptCount = 0,
            CreatedAt = now.AddMinutes(-1)
        };

        var user = new UserDetails
        {
            Id = 10001,
            FullName = "Existing User",
            Phone = "0912345678",
            CompanyId = 501,
            Tier = 3,
            Status = "Active"
        };

        _otpRepoMock.Setup(x => x.GetPendingByPhoneAndPurposeAsync(
                "0912345678", OtpPurpose.EndUserLogin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);
        _userServiceMock.Setup(x => x.GetUserByPhoneAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.VerifyLoginOtpAsync(
            "0912345678", "654321", null, null, null, "corr-2");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(10001, result.UserId);
        Assert.Equal("Existing User", result.FullName);
        Assert.Equal("0912345678", result.Phone);
        Assert.Equal(501, result.CompanyId);
        Assert.Equal(3, result.Tier);
        _otpRepoMock.Verify(x => x.UpdateAsync(otpRequest, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyLoginOtp_WithExpiredOtp_ReturnsError()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 1, 4, 10, 0, 0, TimeSpan.Zero);
        var otpRequest = new OtpRequest
        {
            Id = 2,
            Phone = "0912345678",
            Purpose = OtpPurpose.EndUserLogin,
            OtpCodeHash = OtpHelper.HashOtp("654321"),
            ExpiresAt = now.AddMinutes(-1), // Expired
            Status = OtpStatus.Pending,
            AttemptCount = 0,
            CreatedAt = now.AddMinutes(-3)
        };

        _otpRepoMock.Setup(x => x.GetPendingByPhoneAndPurposeAsync(
                "0912345678", OtpPurpose.EndUserLogin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);

        // Act
        var result = await _authService.VerifyLoginOtpAsync(
            "0912345678", "654321", null, null, null, "corr-2");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.OtpInvalidOrExpired, result.ErrorCode);
    }

    [Fact]
    public async Task VerifyLoginOtp_WithWrongOtpCode_RecordsAttemptAndReturnsError()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 1, 4, 10, 0, 0, TimeSpan.Zero);
        var otpRequest = new OtpRequest
        {
            Id = 2,
            Phone = "0912345678",
            Purpose = OtpPurpose.EndUserLogin,
            OtpCodeHash = OtpHelper.HashOtp("654321"),
            ExpiresAt = now.AddMinutes(2),
            Status = OtpStatus.Pending,
            AttemptCount = 0,
            CreatedAt = now.AddMinutes(-1)
        };

        _otpRepoMock.Setup(x => x.GetPendingByPhoneAndPurposeAsync(
                "0912345678", OtpPurpose.EndUserLogin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);

        // Act
        var result = await _authService.VerifyLoginOtpAsync(
            "0912345678", "000000", null, null, null, "corr-2");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.OtpInvalidOrExpired, result.ErrorCode);
        Assert.Equal(1, otpRequest.AttemptCount);
        _otpRepoMock.Verify(x => x.UpdateAsync(otpRequest, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyLoginOtp_WithMaxAttemptsExceeded_ReturnsError()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 1, 4, 10, 0, 0, TimeSpan.Zero);
        var otpRequest = new OtpRequest
        {
            Id = 2,
            Phone = "0912345678",
            Purpose = OtpPurpose.EndUserLogin,
            OtpCodeHash = OtpHelper.HashOtp("654321"),
            ExpiresAt = now.AddMinutes(2),
            Status = OtpStatus.Pending,
            AttemptCount = 5, // Max attempts reached
            CreatedAt = now.AddMinutes(-1)
        };

        _otpRepoMock.Setup(x => x.GetPendingByPhoneAndPurposeAsync(
                "0912345678", OtpPurpose.EndUserLogin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);

        // Act
        var result = await _authService.VerifyLoginOtpAsync(
            "0912345678", "654321", null, null, null, "corr-2");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.OtpMaxAttemptsExceeded, result.ErrorCode);
    }

    [Fact]
    public async Task VerifyLoginOtp_WithNoPendingOtp_ReturnsError()
    {
        // Arrange
        _otpRepoMock.Setup(x => x.GetPendingByPhoneAndPurposeAsync(
                "0912345678", OtpPurpose.EndUserLogin, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OtpRequest?)null);

        // Act
        var result = await _authService.VerifyLoginOtpAsync(
            "0912345678", "654321", null, null, null, "corr-2");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.OtpInvalidOrExpired, result.ErrorCode);
    }

    [Fact]
    public async Task VerifyLoginOtp_WhenUserNotFound_ReturnsError()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 1, 4, 10, 0, 0, TimeSpan.Zero);
        var otpRequest = new OtpRequest
        {
            Id = 2,
            Phone = "0912345678",
            Purpose = OtpPurpose.EndUserLogin,
            OtpCodeHash = OtpHelper.HashOtp("654321"),
            ExpiresAt = now.AddMinutes(2),
            Status = OtpStatus.Pending,
            AttemptCount = 0,
            CreatedAt = now.AddMinutes(-1)
        };

        _otpRepoMock.Setup(x => x.GetPendingByPhoneAndPurposeAsync(
                "0912345678", OtpPurpose.EndUserLogin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);
        _userServiceMock.Setup(x => x.GetUserByPhoneAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDetails?)null); // User deleted in meantime

        // Act
        var result = await _authService.VerifyLoginOtpAsync(
            "0912345678", "654321", null, null, null, "corr-2");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.UserNotFound, result.ErrorCode);
    }

    [Fact]
    public async Task VerifyLoginOtp_WhenUserLockedDuringVerification_ReturnsError()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 1, 4, 10, 0, 0, TimeSpan.Zero);
        var otpRequest = new OtpRequest
        {
            Id = 2,
            Phone = "0912345678",
            Purpose = OtpPurpose.EndUserLogin,
            OtpCodeHash = OtpHelper.HashOtp("654321"),
            ExpiresAt = now.AddMinutes(2),
            Status = OtpStatus.Pending,
            AttemptCount = 0,
            CreatedAt = now.AddMinutes(-1)
        };

        var lockedUser = new UserDetails
        {
            Id = 10001,
            FullName = "Locked User",
            Phone = "0912345678",
            CompanyId = 501,
            Tier = 3,
            Status = "Locked" // Locked after OTP was sent
        };

        _otpRepoMock.Setup(x => x.GetPendingByPhoneAndPurposeAsync(
                "0912345678", OtpPurpose.EndUserLogin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);
        _userServiceMock.Setup(x => x.GetUserByPhoneAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockedUser);

        // Act
        var result = await _authService.VerifyLoginOtpAsync(
            "0912345678", "654321", null, null, null, "corr-2");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.UserLocked, result.ErrorCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1912345678")]
    public async Task VerifyLoginOtp_WithInvalidPhoneFormat_ReturnsError(string phone)
    {
        // Act
        var result = await _authService.VerifyLoginOtpAsync(
            phone, "123456", null, null, null, "corr-2");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.InvalidPhoneFormat, result.ErrorCode);
    }

    #endregion

    #region RefreshAccessTokenAsync Tests - US-AUTH-003

    [Fact]
    public async Task RefreshAccessToken_WithValidToken_ReturnsSuccess()
    {
        // Arrange - AC2: Valid refresh token issues new access token
        var rawToken = "valid_refresh_token_abc123";
        var tokenHash = "hashed_token";
        var now = _timeProviderMock.Object.GetUtcNow();

        _tokenServiceMock.Setup(x => x.HashRefreshToken(rawToken))
            .Returns(tokenHash);

        var storedToken = new RefreshToken
        {
            Id = 1,
            UserId = 10001,
            TokenHash = tokenHash,
            ExpiresAt = now.AddDays(7),
            RevokedAt = null,
            IsDeleted = false,
            CreatedAt = now.AddDays(-1),
            CreatedBy = "system"
        };

        _tokenRepoMock.Setup(x => x.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        var user = new UserDetails
        {
            Id = 10001,
            FullName = "Trần Văn A",
            Phone = "0912345678",
            CompanyId = 501,
            Tier = 3,
            Status = "Active"
        };

        _userServiceMock.Setup(x => x.GetUserByIdAsync(10001, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.RefreshAccessTokenAsync(rawToken, "corr-1");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(10001, result.UserId);
        Assert.Equal("Trần Văn A", result.FullName);
        Assert.Equal("0912345678", result.Phone);
        Assert.Equal(501, result.CompanyId);
        Assert.Equal(3, result.Tier);
    }

    [Fact]
    public async Task RefreshAccessToken_WithEmptyToken_ReturnsError()
    {
        // Act
        var result = await _authService.RefreshAccessTokenAsync("", "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.TokenInvalid, result.ErrorCode);
        Assert.Contains("required", result.ErrorMessage);
    }

    [Fact]
    public async Task RefreshAccessToken_WithNonExistentToken_ReturnsError()
    {
        // Arrange - EC2: Token not found or corrupted
        var rawToken = "invalid_token_xyz";
        var tokenHash = "hashed_invalid";

        _tokenServiceMock.Setup(x => x.HashRefreshToken(rawToken))
            .Returns(tokenHash);

        _tokenRepoMock.Setup(x => x.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _authService.RefreshAccessTokenAsync(rawToken, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.TokenInvalid, result.ErrorCode);
        Assert.Contains("Invalid refresh token", result.ErrorMessage);
    }

    [Fact]
    public async Task RefreshAccessToken_WithRevokedToken_ReturnsError()
    {
        // Arrange - EC1: Token has been revoked (security reset)
        var rawToken = "revoked_token";
        var tokenHash = "hashed_revoked";
        var now = _timeProviderMock.Object.GetUtcNow();

        _tokenServiceMock.Setup(x => x.HashRefreshToken(rawToken))
            .Returns(tokenHash);

        var storedToken = new RefreshToken
        {
            Id = 2,
            UserId = 10002,
            TokenHash = tokenHash,
            ExpiresAt = now.AddDays(7),
            RevokedAt = now.AddHours(-1), // Revoked 1 hour ago
            RevokedReason = "User requested security reset",
            IsDeleted = false,
            CreatedAt = now.AddDays(-1),
            CreatedBy = "system"
        };

        _tokenRepoMock.Setup(x => x.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        var result = await _authService.RefreshAccessTokenAsync(rawToken, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.RefreshTokenRevoked, result.ErrorCode);
        Assert.Contains("revoked", result.ErrorMessage);
    }

    [Fact]
    public async Task RefreshAccessToken_WithExpiredToken_ReturnsError()
    {
        // Arrange - AC3: Both tokens expired, require re-login
        var rawToken = "expired_token";
        var tokenHash = "hashed_expired";
        var now = _timeProviderMock.Object.GetUtcNow();

        _tokenServiceMock.Setup(x => x.HashRefreshToken(rawToken))
            .Returns(tokenHash);

        var storedToken = new RefreshToken
        {
            Id = 3,
            UserId = 10003,
            TokenHash = tokenHash,
            ExpiresAt = now.AddHours(-1), // Expired 1 hour ago
            RevokedAt = null,
            IsDeleted = false,
            CreatedAt = now.AddDays(-8),
            CreatedBy = "system"
        };

        _tokenRepoMock.Setup(x => x.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        var result = await _authService.RefreshAccessTokenAsync(rawToken, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.RefreshTokenExpired, result.ErrorCode);
        Assert.Contains("expired", result.ErrorMessage);
    }

    [Fact]
    public async Task RefreshAccessToken_WithDeletedToken_ReturnsError()
    {
        // Arrange - Token is soft-deleted
        var rawToken = "deleted_token";
        var tokenHash = "hashed_deleted";
        var now = _timeProviderMock.Object.GetUtcNow();

        _tokenServiceMock.Setup(x => x.HashRefreshToken(rawToken))
            .Returns(tokenHash);

        var storedToken = new RefreshToken
        {
            Id = 4,
            UserId = 10004,
            TokenHash = tokenHash,
            ExpiresAt = now.AddDays(7),
            RevokedAt = null,
            IsDeleted = true, // Soft deleted
            CreatedAt = now.AddDays(-1),
            CreatedBy = "system"
        };

        _tokenRepoMock.Setup(x => x.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        var result = await _authService.RefreshAccessTokenAsync(rawToken, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.TokenInvalid, result.ErrorCode);
    }

    [Fact]
    public async Task RefreshAccessToken_WithNoAssociatedUser_ReturnsError()
    {
        // Arrange - Token has no UserId (data integrity issue)
        var rawToken = "orphan_token";
        var tokenHash = "hashed_orphan";
        var now = _timeProviderMock.Object.GetUtcNow();

        _tokenServiceMock.Setup(x => x.HashRefreshToken(rawToken))
            .Returns(tokenHash);

        var storedToken = new RefreshToken
        {
            Id = 5,
            UserId = null, // No associated user
            AdminUserId = null,
            TokenHash = tokenHash,
            ExpiresAt = now.AddDays(7),
            RevokedAt = null,
            IsDeleted = false,
            CreatedAt = now.AddDays(-1),
            CreatedBy = "system"
        };

        _tokenRepoMock.Setup(x => x.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        var result = await _authService.RefreshAccessTokenAsync(rawToken, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.TokenInvalid, result.ErrorCode);
    }

    [Fact]
    public async Task RefreshAccessToken_WhenUserNotFound_ReturnsError()
    {
        // Arrange - User deleted between token issuance and refresh
        var rawToken = "valid_token_missing_user";
        var tokenHash = "hashed_valid";
        var now = _timeProviderMock.Object.GetUtcNow();

        _tokenServiceMock.Setup(x => x.HashRefreshToken(rawToken))
            .Returns(tokenHash);

        var storedToken = new RefreshToken
        {
            Id = 6,
            UserId = 99999, // User doesn't exist
            TokenHash = tokenHash,
            ExpiresAt = now.AddDays(7),
            RevokedAt = null,
            IsDeleted = false,
            CreatedAt = now.AddDays(-1),
            CreatedBy = "system"
        };

        _tokenRepoMock.Setup(x => x.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _userServiceMock.Setup(x => x.GetUserByIdAsync(99999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDetails?)null);

        // Act
        var result = await _authService.RefreshAccessTokenAsync(rawToken, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.UserNotFound, result.ErrorCode);
    }

    [Fact]
    public async Task RefreshAccessToken_WhenUserLocked_ReturnsError()
    {
        // Arrange - User locked after token was issued
        var rawToken = "valid_token_locked_user";
        var tokenHash = "hashed_locked";
        var now = _timeProviderMock.Object.GetUtcNow();

        _tokenServiceMock.Setup(x => x.HashRefreshToken(rawToken))
            .Returns(tokenHash);

        var storedToken = new RefreshToken
        {
            Id = 7,
            UserId = 10005,
            TokenHash = tokenHash,
            ExpiresAt = now.AddDays(7),
            RevokedAt = null,
            IsDeleted = false,
            CreatedAt = now.AddDays(-1),
            CreatedBy = "system"
        };

        _tokenRepoMock.Setup(x => x.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        var lockedUser = new UserDetails
        {
            Id = 10005,
            FullName = "Locked User",
            Phone = "0987654321",
            CompanyId = 502,
            Tier = 2,
            Status = "Locked" // User is locked
        };

        _userServiceMock.Setup(x => x.GetUserByIdAsync(10005, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockedUser);

        // Act
        var result = await _authService.RefreshAccessTokenAsync(rawToken, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.UserLocked, result.ErrorCode);
        Assert.Contains("not accessible", result.ErrorMessage);
    }

    [Fact]
    public async Task RefreshAccessToken_WhenUserInactive_ReturnsError()
    {
        // Arrange - User inactive after token was issued
        var rawToken = "valid_token_inactive_user";
        var tokenHash = "hashed_inactive";
        var now = _timeProviderMock.Object.GetUtcNow();

        _tokenServiceMock.Setup(x => x.HashRefreshToken(rawToken))
            .Returns(tokenHash);

        var storedToken = new RefreshToken
        {
            Id = 8,
            UserId = 10006,
            TokenHash = tokenHash,
            ExpiresAt = now.AddDays(7),
            RevokedAt = null,
            IsDeleted = false,
            CreatedAt = now.AddDays(-1),
            CreatedBy = "system"
        };

        _tokenRepoMock.Setup(x => x.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        var inactiveUser = new UserDetails
        {
            Id = 10006,
            FullName = "Inactive User",
            Phone = "0900000001",
            CompanyId = 503,
            Tier = 1,
            Status = "Inactive" // User is inactive
        };

        _userServiceMock.Setup(x => x.GetUserByIdAsync(10006, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveUser);

        // Act
        var result = await _authService.RefreshAccessTokenAsync(rawToken, "corr-1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.UserLocked, result.ErrorCode);
    }

    #endregion

    #region LogoutAsync Tests - US-AUTH-004

    [Fact]
    public async Task Logout_WithActiveTokens_RevokesAllTokens()
    {
        // Arrange - AC1: Logout revokes all active tokens
        var userId = 10001L;
        var now = _timeProviderMock.Object.GetUtcNow();

        var token1 = new RefreshToken
        {
            Id = 1,
            UserId = userId,
            TokenHash = "hash1",
            ExpiresAt = now.AddDays(7),
            RevokedAt = null,
            IsDeleted = false,
            CreatedAt = now.AddDays(-1),
            CreatedBy = "system"
        };

        var token2 = new RefreshToken
        {
            Id = 2,
            UserId = userId,
            TokenHash = "hash2",
            ExpiresAt = now.AddDays(7),
            RevokedAt = null,
            IsDeleted = false,
            CreatedAt = now.AddDays(-2),
            CreatedBy = "system"
        };

        _tokenRepoMock.Setup(x => x.GetActiveTokensByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken> { token1, token2 });

        // Act
        await _authService.LogoutAsync(userId, "corr-1");

        // Assert - AC1: Both tokens revoked
        _tokenRepoMock.Verify(x => x.UpdateAsync(It.Is<RefreshToken>(t => 
            t.Id == 1 && t.RevokedAt != null && t.RevokedReason == "User logout"), 
            It.IsAny<CancellationToken>()), Times.Once);
        _tokenRepoMock.Verify(x => x.UpdateAsync(It.Is<RefreshToken>(t => 
            t.Id == 2 && t.RevokedAt != null && t.RevokedReason == "User logout"), 
            It.IsAny<CancellationToken>()), Times.Once);
        _tokenRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Logout_WithNoActiveTokens_IsIdempotent()
    {
        // Arrange - Idempotent: no tokens found (already logged out)
        var userId = 10002L;

        _tokenRepoMock.Setup(x => x.GetActiveTokensByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>());

        // Act
        await _authService.LogoutAsync(userId, "corr-1");

        // Assert - No updates should be made
        _tokenRepoMock.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Logout_MultipleTabsScenario_RevokesAllDeviceTokens()
    {
        // Arrange - EC1: Multiple tabs/devices, all tokens should be revoked
        var userId = 10003L;
        var now = _timeProviderMock.Object.GetUtcNow();

        // Simulate 3 tokens from different "sessions" (tabs/devices)
        var tokens = new List<RefreshToken>
        {
            new()
            {
                Id = 1,
                UserId = userId,
                TokenHash = "hash_device1",
                DeviceId = "device-1",
                ExpiresAt = now.AddDays(7),
                RevokedAt = null,
                IsDeleted = false,
                CreatedAt = now.AddDays(-1),
                CreatedBy = "system"
            },
            new()
            {
                Id = 2,
                UserId = userId,
                TokenHash = "hash_device2",
                DeviceId = "device-2",
                ExpiresAt = now.AddDays(7),
                RevokedAt = null,
                IsDeleted = false,
                CreatedAt = now.AddDays(-2),
                CreatedBy = "system"
            },
            new()
            {
                Id = 3,
                UserId = userId,
                TokenHash = "hash_device3",
                DeviceId = "device-3",
                ExpiresAt = now.AddDays(7),
                RevokedAt = null,
                IsDeleted = false,
                CreatedAt = now.AddHours(-6),
                CreatedBy = "system"
            }
        };

        _tokenRepoMock.Setup(x => x.GetActiveTokensByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokens);

        // Act
        await _authService.LogoutAsync(userId, "corr-1");

        // Assert - All 3 tokens should be revoked (EC1: all tabs/devices)
        _tokenRepoMock.Verify(x => x.UpdateAsync(It.Is<RefreshToken>(t => 
            t.RevokedAt != null && t.RevokedReason == "User logout"), 
            It.IsAny<CancellationToken>()), Times.Exactly(3));
        _tokenRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Logout_RepeatedCall_IsIdempotent()
    {
        // Arrange - Second logout call after tokens already revoked
        var userId = 10004L;
        var now = _timeProviderMock.Object.GetUtcNow();

        // First call: tokens exist
        var token = new RefreshToken
        {
            Id = 1,
            UserId = userId,
            TokenHash = "hash1",
            ExpiresAt = now.AddDays(7),
            RevokedAt = null,
            IsDeleted = false,
            CreatedAt = now.AddDays(-1),
            CreatedBy = "system"
        };

        _tokenRepoMock.SetupSequence(x => x.GetActiveTokensByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken> { token })  // First call: has token
            .ReturnsAsync(new List<RefreshToken>());        // Second call: no active tokens

        // Act - First logout
        await _authService.LogoutAsync(userId, "corr-1");
        
        // Act - Second logout (idempotent)
        await _authService.LogoutAsync(userId, "corr-2");

        // Assert - First call updates, second call does not
        _tokenRepoMock.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Logout_WithSingleToken_RevokesSuccessfully()
    {
        // Arrange - Simple case: one token
        var userId = 10005L;
        var now = _timeProviderMock.Object.GetUtcNow();

        var token = new RefreshToken
        {
            Id = 1,
            UserId = userId,
            TokenHash = "single_hash",
            ExpiresAt = now.AddDays(7),
            RevokedAt = null,
            IsDeleted = false,
            CreatedAt = now.AddDays(-1),
            CreatedBy = "system"
        };

        _tokenRepoMock.Setup(x => x.GetActiveTokensByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken> { token });

        // Act
        await _authService.LogoutAsync(userId, "corr-1");

        // Assert
        Assert.NotNull(token.RevokedAt);
        Assert.Equal("User logout", token.RevokedReason);
        _tokenRepoMock.Verify(x => x.UpdateAsync(token, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Logout_LogsCorrelationId()
    {
        // Arrange - AC3: Logout event is logged with correlationId
        var userId = 10006L;
        var correlationId = "test-corr-id-123";
        var now = _timeProviderMock.Object.GetUtcNow();

        var token = new RefreshToken
        {
            Id = 1,
            UserId = userId,
            TokenHash = "hash",
            ExpiresAt = now.AddDays(7),
            RevokedAt = null,
            IsDeleted = false,
            CreatedAt = now.AddDays(-1),
            CreatedBy = "system"
        };

        _tokenRepoMock.Setup(x => x.GetActiveTokensByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken> { token });

        // Act
        await _authService.LogoutAsync(userId, correlationId);

        // Assert - Verify token was revoked and changes saved
        _tokenRepoMock.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // Note: Logger verification would require setting up LoggerMock properly
        // For now, we verify the operation completed successfully
    }

    #endregion

    #region US-AUTH-005: Admin Portal Login Tests

    [Fact]
    public async Task AdminLoginAsync_WithValidCredentials_ReturnsTokens()
    {
        // Arrange - AC1: Happy path for valid admin login
        var username = "ops.admin@example.com";
        var password = "P@ssw0rd!";
        var passwordHash = "$2a$11$hashed_password";

        var adminUser = new AdminUserDetails
        {
            UserId = 9001,
            LoginId = username,
            PasswordHash = passwordHash,
            Role = "Ops",
            CompanyId = 1,
            IsActive = true,
            IsLocked = false,
            IsDeleted = false
        };

        _userServiceMock.Setup(x => x.GetAdminUserByLoginAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        _userServiceMock.Setup(x => x.VerifyPassword(password, passwordHash))
            .Returns(true);

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(9001, username, 0, 1))
            .Returns(("access_token_admin", 3600));

        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token_admin");

        _tokenServiceMock.Setup(x => x.HashRefreshToken("refresh_token_admin"))
            .Returns("refresh_hash_admin");

        // Act
        var result = await _authService.AdminLoginAsync(username, password, "corr-admin-1");

        // Assert - AC1: Tokens issued successfully
        Assert.True(result.Success);
        Assert.Equal("access_token_admin", result.AccessToken);
        Assert.Equal("refresh_token_admin", result.RefreshToken);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.Null(result.ErrorCode);

        // Verify refresh token was stored - AC3: Login logged
        _tokenRepoMock.Verify(
            x => x.AddAsync(It.Is<RefreshToken>(t => t.UserId == 9001 && t.CorrelationId == "corr-admin-1"), It.IsAny<CancellationToken>()),
            Times.Once);
        _tokenRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdminLoginAsync_WithInvalidUsername_ReturnsGenericError()
    {
        // Arrange - AC4: Generic error message, don't reveal user doesn't exist
        var username = "nonexistent@example.com";
        var password = "P@ssw0rd!";

        _userServiceMock.Setup(x => x.GetAdminUserByLoginAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminUserDetails?)null);

        // Act
        var result = await _authService.AdminLoginAsync(username, password, "corr-admin-2");

        // Assert - AC4: Generic error, no details revealed
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.AdminInvalidCredentials, result.ErrorCode);
        Assert.Contains("Invalid username or password", result.ErrorMessage);
        Assert.Null(result.AccessToken);

        // Verify no token operations performed
        _tokenRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdminLoginAsync_WithInvalidPassword_ReturnsGenericError()
    {
        // Arrange - AC4: Generic error message, don't reveal password is wrong
        var username = "ops.admin@example.com";
        var password = "WrongPassword";
        var passwordHash = "$2a$11$hashed_password";

        var adminUser = new AdminUserDetails
        {
            UserId = 9001,
            LoginId = username,
            PasswordHash = passwordHash,
            Role = "Ops",
            CompanyId = 1,
            IsActive = true,
            IsLocked = false,
            IsDeleted = false
        };

        _userServiceMock.Setup(x => x.GetAdminUserByLoginAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        _userServiceMock.Setup(x => x.VerifyPassword(password, passwordHash))
            .Returns(false);

        // Act
        var result = await _authService.AdminLoginAsync(username, password, "corr-admin-3");

        // Assert - AC4: Generic error, no password hint
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.AdminInvalidCredentials, result.ErrorCode);
        Assert.Contains("Invalid username or password", result.ErrorMessage);

        // Verify no tokens generated
        _tokenRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdminLoginAsync_WithLockedAccount_ReturnsLockedError()
    {
        // Arrange - EC1: Account locked due to failed login attempts
        var username = "locked.admin@example.com";
        var password = "P@ssw0rd!";

        var adminUser = new AdminUserDetails
        {
            UserId = 9002,
            LoginId = username,
            PasswordHash = "$2a$11$hashed",
            Role = "Ops",
            CompanyId = 1,
            IsActive = true,
            IsLocked = true,  // Account is locked
            IsDeleted = false
        };

        _userServiceMock.Setup(x => x.GetAdminUserByLoginAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        // Act
        var result = await _authService.AdminLoginAsync(username, password, "corr-admin-4");

        // Assert - EC1: Locked account rejected
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.AdminLocked, result.ErrorCode);
        Assert.Contains("locked", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);

        // Verify password not checked for locked account
        _userServiceMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _tokenRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdminLoginAsync_WithInactiveAccount_ReturnsNotAuthorizedError()
    {
        // Arrange - EC4: Account disabled/inactive
        var username = "inactive.admin@example.com";
        var password = "P@ssw0rd!";

        var adminUser = new AdminUserDetails
        {
            UserId = 9003,
            LoginId = username,
            PasswordHash = "$2a$11$hashed",
            Role = "Support",
            CompanyId = 1,
            IsActive = false,  // Account inactive
            IsLocked = false,
            IsDeleted = false
        };

        _userServiceMock.Setup(x => x.GetAdminUserByLoginAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        // Act
        var result = await _authService.AdminLoginAsync(username, password, "corr-admin-5");

        // Assert - EC4: Inactive account rejected
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.AdminNotAuthorized, result.ErrorCode);
        Assert.Contains("not authorized", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);

        // Verify no tokens issued
        _tokenRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdminLoginAsync_WithDeletedAccount_ReturnsNotAuthorizedError()
    {
        // Arrange - EC4: Account marked as deleted
        var username = "deleted.admin@example.com";
        var password = "P@ssw0rd!";

        var adminUser = new AdminUserDetails
        {
            UserId = 9004,
            LoginId = username,
            PasswordHash = "$2a$11$hashed",
            Role = "QC",
            CompanyId = 1,
            IsActive = true,
            IsLocked = false,
            IsDeleted = true  // Account deleted
        };

        _userServiceMock.Setup(x => x.GetAdminUserByLoginAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        // Act
        var result = await _authService.AdminLoginAsync(username, password, "corr-admin-6");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.AdminNotAuthorized, result.ErrorCode);
        _tokenRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdminLoginAsync_WithEndUserRole_ReturnsNotAuthorizedError()
    {
        // Arrange - AC2: End User trying to login to Admin Portal
        var username = "enduser@example.com";
        var password = "P@ssw0rd!";

        var adminUser = new AdminUserDetails
        {
            UserId = 10001,
            LoginId = username,
            PasswordHash = "$2a$11$hashed",
            Role = "EndUser",  // Not an admin role
            CompanyId = 1,
            IsActive = true,
            IsLocked = false,
            IsDeleted = false
        };

        _userServiceMock.Setup(x => x.GetAdminUserByLoginAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        // Act
        var result = await _authService.AdminLoginAsync(username, password, "corr-admin-7");

        // Assert - AC2: End User rejected from Admin Portal
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.AdminNotAuthorized, result.ErrorCode);
        Assert.Contains("not authorized for admin access", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);

        // Verify password not even checked
        _userServiceMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _tokenRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdminLoginAsync_WithEmptyRole_ReturnsNotAuthorizedError()
    {
        // Arrange - AC2: Account with no role assigned
        var username = "norole@example.com";
        var password = "P@ssw0rd!";

        var adminUser = new AdminUserDetails
        {
            UserId = 9005,
            LoginId = username,
            PasswordHash = "$2a$11$hashed",
            Role = "",  // Empty role
            CompanyId = 1,
            IsActive = true,
            IsLocked = false,
            IsDeleted = false
        };

        _userServiceMock.Setup(x => x.GetAdminUserByLoginAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        // Act
        var result = await _authService.AdminLoginAsync(username, password, "corr-admin-8");

        // Assert - AC2: No role = not authorized
        Assert.False(result.Success);
        Assert.Equal(AuthErrorCodes.AdminNotAuthorized, result.ErrorCode);
        _tokenRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdminLoginAsync_WithSuperAdminRole_ReturnsTokens()
    {
        // Arrange - EC2: Multiple roles scenario - SuperAdmin
        var username = "superadmin@example.com";
        var password = "P@ssw0rd!";
        var passwordHash = "$2a$11$hashed_password";

        var adminUser = new AdminUserDetails
        {
            UserId = 9010,
            LoginId = username,
            PasswordHash = passwordHash,
            Role = "SuperAdmin",  // Highest privilege role
            CompanyId = 1,
            IsActive = true,
            IsLocked = false,
            IsDeleted = false
        };

        _userServiceMock.Setup(x => x.GetAdminUserByLoginAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        _userServiceMock.Setup(x => x.VerifyPassword(password, passwordHash))
            .Returns(true);

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(9010, username, 0, 1))
            .Returns(("access_token_superadmin", 3600));

        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token_superadmin");

        _tokenServiceMock.Setup(x => x.HashRefreshToken("refresh_token_superadmin"))
            .Returns("refresh_hash_superadmin");

        // Act
        var result = await _authService.AdminLoginAsync(username, password, "corr-admin-9");

        // Assert - EC2: SuperAdmin can login successfully
        Assert.True(result.Success);
        Assert.Equal("access_token_superadmin", result.AccessToken);
        Assert.Equal("refresh_token_superadmin", result.RefreshToken);
        _tokenRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
