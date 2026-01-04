using CoShare.Domain.Auth;
using Microsoft.Extensions.Logging;

namespace CoShare.Infrastructure.Services;

/// <summary>
/// Stub implementation of IUserService for USERS module integration.
/// In production, this will call the actual USERS repository/service.
/// </summary>
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;

    // In-memory store for demo/testing (replace with actual DB in production)
    private static readonly Dictionary<string, UserDetails> _usersByPhone = new();
    private static readonly HashSet<long> _validCompanyIds = new() { 501, 502, 503 };
    private static long _nextUserId = 10001;
    private static readonly object _lock = new();

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public Task<bool> PhoneExistsAsync(string phone, CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_usersByPhone.ContainsKey(phone));
        }
    }

    public Task<bool> CompanyExistsAsync(long companyId, CancellationToken ct = default)
    {
        // In production, check admin_company table
        return Task.FromResult(_validCompanyIds.Contains(companyId));
    }

    public Task<long> CreateEndUserAsync(
        string phone,
        string fullName,
        long companyId,
        string correlationId,
        CancellationToken ct = default)
    {
        lock (_lock)
        {
            var userId = _nextUserId++;
            var user = new UserDetails
            {
                UserId = userId,
                FullName = fullName,
                Phone = phone,
                CompanyId = companyId,
                Tier = 3 // New end users are Tier 3
            };

            _usersByPhone[phone] = user;

            _logger.LogInformation(
                "End user created. UserId={UserId}, CompanyId={CompanyId}, CorrelationId={CorrelationId}",
                userId, companyId, correlationId);

            return Task.FromResult(userId);
        }
    }

    public Task<UserDetails?> GetUserByIdAsync(long userId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var user = _usersByPhone.Values.FirstOrDefault(u => u.UserId == userId);
            return Task.FromResult(user);
        }
    }
}
