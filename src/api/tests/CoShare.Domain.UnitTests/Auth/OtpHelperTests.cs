using CoShare.Domain.Auth;

namespace CoShare.Domain.UnitTests.Auth;

/// <summary>
/// Unit tests for OTP helper functions.
/// Test ID: UT-AUTH-001 (part of OTP logic validation)
/// </summary>
public class OtpHelperTests
{
    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    public void GenerateOtp_ReturnsCorrectLength(int length)
    {
        // Act
        var otp = OtpHelper.GenerateOtp(length);

        // Assert
        Assert.Equal(length, otp.Length);
        Assert.Matches(@"^\d+$", otp);
    }

    [Fact]
    public void GenerateOtp_WithZeroLength_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => OtpHelper.GenerateOtp(0));
    }

    [Fact]
    public void GenerateOtp_WithTooLargeLength_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => OtpHelper.GenerateOtp(11));
    }

    [Fact]
    public void HashOtp_ReturnsNonEmptyHash()
    {
        // Act
        var hash = OtpHelper.HashOtp("123456");

        // Assert
        Assert.NotEmpty(hash);
        Assert.NotEqual("123456", hash); // Hash should not be the same as input
    }

    [Fact]
    public void VerifyOtp_WithCorrectCode_ReturnsTrue()
    {
        // Arrange
        var code = "123456";
        var hash = OtpHelper.HashOtp(code);

        // Act
        var result = OtpHelper.VerifyOtp(code, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyOtp_WithWrongCode_ReturnsFalse()
    {
        // Arrange
        var hash = OtpHelper.HashOtp("123456");

        // Act
        var result = OtpHelper.VerifyOtp("654321", hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsBase64String()
    {
        // Act
        var token = OtpHelper.GenerateRefreshToken();

        // Assert
        Assert.NotEmpty(token);
        // Should be valid base64
        var bytes = Convert.FromBase64String(token);
        Assert.Equal(32, bytes.Length);
    }

    [Fact]
    public void HashRefreshToken_ReturnsDeterministicHash()
    {
        // Arrange
        var token = "test-token-value";

        // Act
        var hash1 = OtpHelper.HashRefreshToken(token);
        var hash2 = OtpHelper.HashRefreshToken(token);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashRefreshToken_DifferentTokens_ReturnsDifferentHashes()
    {
        // Act
        var hash1 = OtpHelper.HashRefreshToken("token-1");
        var hash2 = OtpHelper.HashRefreshToken("token-2");

        // Assert
        Assert.NotEqual(hash1, hash2);
    }
}
