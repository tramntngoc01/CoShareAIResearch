using CoShare.Domain.Auth;
using CoShare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoShare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for OTP requests.
/// </summary>
public class OtpRequestRepository : IOtpRequestRepository
{
    private readonly AuthDbContext _context;

    public OtpRequestRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<OtpRequest?> GetPendingByPhoneAndPurposeAsync(
        string phone,
        OtpPurpose purpose,
        CancellationToken ct = default)
    {
        return await _context.OtpRequests
            .Where(o => o.Phone == phone
                        && o.Purpose == purpose
                        && o.Status == OtpStatus.Pending)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CountRecentRequestsAsync(
        string phone,
        OtpPurpose purpose,
        DateTimeOffset since,
        CancellationToken ct = default)
    {
        return await _context.OtpRequests
            .IgnoreQueryFilters() // Count all including deleted for rate limiting
            .Where(o => o.Phone == phone
                        && o.Purpose == purpose
                        && o.CreatedAt >= since)
            .CountAsync(ct);
    }

    public async Task AddAsync(OtpRequest otpRequest, CancellationToken ct = default)
    {
        await _context.OtpRequests.AddAsync(otpRequest, ct);
    }

    public Task UpdateAsync(OtpRequest otpRequest, CancellationToken ct = default)
    {
        _context.OtpRequests.Update(otpRequest);
        return Task.CompletedTask;
    }

    public async Task CancelPendingByPhoneAndPurposeAsync(
        string phone,
        OtpPurpose purpose,
        DateTimeOffset now,
        string? updatedBy,
        CancellationToken ct = default)
    {
        var pendingOtps = await _context.OtpRequests
            .Where(o => o.Phone == phone
                        && o.Purpose == purpose
                        && o.Status == OtpStatus.Pending)
            .ToListAsync(ct);

        foreach (var otp in pendingOtps)
        {
            otp.Cancel(now, updatedBy);
        }
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
