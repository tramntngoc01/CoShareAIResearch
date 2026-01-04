namespace CoShare.Domain.Auth;

/// <summary>
/// Purpose of an OTP request per DB-AUTH.
/// </summary>
public enum OtpPurpose
{
    EndUserRegister,
    EndUserLoginFirstTime
}
