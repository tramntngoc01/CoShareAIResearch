using CoShare.Domain.Auth;

namespace CoShare.Domain.UnitTests.Auth;

/// <summary>
/// Unit tests for OtpRequest entity business rules.
/// Test ID: UT-AUTH-001 (part of OTP TTL validation)
/// </summary>
public class OtpRequestTests
{
    [Fact]
    public void IsValidForVerification_WhenPendingAndNotExpired_ReturnsTrue()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var otp = new OtpRequest
        {
            Phone = "0912345678",
            OtpCodeHash = "hash",
            Status = OtpStatus.Pending,
            ExpiresAt = now.AddMinutes(2),
            IsDeleted = false
        };

        // Act
        var result = otp.IsValidForVerification(now);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidForVerification_WhenExpired_ReturnsFalse()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var otp = new OtpRequest
        {
            Phone = "0912345678",
            OtpCodeHash = "hash",
            Status = OtpStatus.Pending,
            ExpiresAt = now.AddMinutes(-1), // Expired
            IsDeleted = false
        };

        // Act
        var result = otp.IsValidForVerification(now);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(OtpStatus.Verified)]
    [InlineData(OtpStatus.Expired)]
    [InlineData(OtpStatus.Cancelled)]
    public void IsValidForVerification_WhenNotPending_ReturnsFalse(OtpStatus status)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var otp = new OtpRequest
        {
            Phone = "0912345678",
            OtpCodeHash = "hash",
            Status = status,
            ExpiresAt = now.AddMinutes(2),
            IsDeleted = false
        };

        // Act
        var result = otp.IsValidForVerification(now);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidForVerification_WhenDeleted_ReturnsFalse()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var otp = new OtpRequest
        {
            Phone = "0912345678",
            OtpCodeHash = "hash",
            Status = OtpStatus.Pending,
            ExpiresAt = now.AddMinutes(2),
            IsDeleted = true
        };

        // Act
        var result = otp.IsValidForVerification(now);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MarkVerified_SetsStatusAndUpdatedAt()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var otp = new OtpRequest
        {
            Phone = "0912345678",
            OtpCodeHash = "hash",
            Status = OtpStatus.Pending,
            ExpiresAt = now.AddMinutes(2)
        };

        // Act
        otp.MarkVerified(now, "system");

        // Assert
        Assert.Equal(OtpStatus.Verified, otp.Status);
        Assert.Equal(now, otp.UpdatedAt);
        Assert.Equal("system", otp.UpdatedBy);
    }

    [Fact]
    public void RecordFailedAttempt_IncrementsCountAndSetsLastAttempt()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var otp = new OtpRequest
        {
            Phone = "0912345678",
            OtpCodeHash = "hash",
            Status = OtpStatus.Pending,
            ExpiresAt = now.AddMinutes(2),
            AttemptCount = 2
        };

        // Act
        otp.RecordFailedAttempt(now, "system");

        // Assert
        Assert.Equal(3, otp.AttemptCount);
        Assert.Equal(now, otp.LastAttemptAt);
        Assert.Equal(now, otp.UpdatedAt);
    }

    [Fact]
    public void Cancel_SetsStatusToCancelled()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var otp = new OtpRequest
        {
            Phone = "0912345678",
            OtpCodeHash = "hash",
            Status = OtpStatus.Pending,
            ExpiresAt = now.AddMinutes(2)
        };

        // Act
        otp.Cancel(now, "system");

        // Assert
        Assert.Equal(OtpStatus.Cancelled, otp.Status);
        Assert.Equal(now, otp.UpdatedAt);
        Assert.Equal("system", otp.UpdatedBy);
    }
}
