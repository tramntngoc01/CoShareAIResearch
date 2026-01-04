using System.Security.Cryptography;

namespace CoShare.Domain.Auth;

/// <summary>
/// OTP generation and hashing utilities.
/// </summary>
public static class OtpHelper
{
    /// <summary>
    /// Generate a random numeric OTP of the specified length.
    /// </summary>
    public static string GenerateOtp(int length)
    {
        if (length <= 0 || length > 10)
            throw new ArgumentOutOfRangeException(nameof(length), "OTP length must be between 1 and 10.");

        var max = (int)Math.Pow(10, length);
        var otp = RandomNumberGenerator.GetInt32(0, max);
        return otp.ToString().PadLeft(length, '0');
    }

    /// <summary>
    /// Hash an OTP code using BCrypt.
    /// </summary>
    public static string HashOtp(string otpCode)
    {
        return BCrypt.Net.BCrypt.HashPassword(otpCode, workFactor: 10);
    }

    /// <summary>
    /// Verify an OTP code against a hash.
    /// </summary>
    public static bool VerifyOtp(string otpCode, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(otpCode, hash);
    }

    /// <summary>
    /// Generate a secure random refresh token.
    /// </summary>
    public static string GenerateRefreshToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Hash a refresh token using SHA256.
    /// </summary>
    public static string HashRefreshToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
