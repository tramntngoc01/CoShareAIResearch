namespace CoShare.Domain.Auth;

/// <summary>
/// Purpose of an OTP request per DB-AUTH (US-AUTH-001, US-AUTH-002).
/// </summary>
public enum OtpPurpose
{
    EndUserRegister,
    EndUserLogin
}
