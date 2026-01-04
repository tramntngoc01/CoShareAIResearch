namespace CoShare.Domain.Auth;

/// <summary>
/// Repository interface for OTP requests.
/// </summary>
public interface IOtpRequestRepository
{
    Task<OtpRequest?> GetPendingByPhoneAndPurposeAsync(
        string phone,
        OtpPurpose purpose,
        CancellationToken ct = default);

    Task<int> CountRecentRequestsAsync(
        string phone,
        OtpPurpose purpose,
        DateTimeOffset since,
        CancellationToken ct = default);

    Task AddAsync(OtpRequest otpRequest, CancellationToken ct = default);

    Task UpdateAsync(OtpRequest otpRequest, CancellationToken ct = default);

    Task CancelPendingByPhoneAndPurposeAsync(
        string phone,
        OtpPurpose purpose,
        DateTimeOffset now,
        string? updatedBy,
        CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
