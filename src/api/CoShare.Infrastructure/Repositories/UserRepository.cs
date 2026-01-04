using CoShare.Domain.Users;
using CoShare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoShare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for User entity.
/// Reference: DB-USERS.md, US-USERS-001 through US-USERS-006
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly UsersDbContext _context;
    private readonly ILogger<UserRepository> _logger;
    private readonly TimeProvider _timeProvider;

    public UserRepository(
        UsersDbContext context,
        ILogger<UserRepository> logger,
        TimeProvider timeProvider)
    {
        _context = context;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<User?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByEmployeeCodeAndCompanyAsync(string employeeCode, long companyId, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.ExternalEmployeeCode == employeeCode && u.CompanyId == companyId, ct);
    }

    public async Task<User?> GetByPhoneAndCompanyAsync(string phone, long companyId, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Phone == phone && u.CompanyId == companyId, ct);
    }

    public async Task<(List<User> items, int totalCount)> SearchAsync(
        long? companyId,
        long? pickupPointId,
        string? tier,
        string? status,
        string? employeeCode,
        string? phone,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.Users.AsQueryable();

        // Apply filters
        if (companyId.HasValue)
            query = query.Where(u => u.CompanyId == companyId.Value);
        
        if (pickupPointId.HasValue)
            query = query.Where(u => u.PickupPointId == pickupPointId.Value);
        
        if (!string.IsNullOrEmpty(tier))
            query = query.Where(u => u.Tier == tier);
        
        if (!string.IsNullOrEmpty(status))
            query = query.Where(u => u.Status == status);
        
        if (!string.IsNullOrEmpty(employeeCode))
            query = query.Where(u => u.ExternalEmployeeCode.Contains(employeeCode));
        
        if (!string.IsNullOrEmpty(phone))
            query = query.Where(u => u.Phone.Contains(phone));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        user.CreatedAt = _timeProvider.GetUtcNow();
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        
        _logger.LogInformation("Created user. UserId={UserId}, EmployeeCode={EmployeeCode}, CompanyId={CompanyId}",
            user.Id, user.ExternalEmployeeCode, user.CompanyId);
        
        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken ct = default)
    {
        user.UpdatedAt = _timeProvider.GetUtcNow();
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
        
        _logger.LogInformation("Updated user. UserId={UserId}", user.Id);
        
        return user;
    }

    public async Task<User> UpdateStatusAsync(long userId, string newStatus, long changedBy, string? reason, CancellationToken ct = default)
    {
        var user = await GetByIdAsync(userId, ct);
        if (user == null)
            throw new InvalidOperationException($"User {userId} not found");

        var oldStatus = user.Status;
        user.Status = newStatus;
        user.UpdatedAt = _timeProvider.GetUtcNow();
        user.UpdatedBy = changedBy;

        // Record status history
        var history = new UserStatusHistory
        {
            UserId = userId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Reason = reason,
            ChangedAt = _timeProvider.GetUtcNow(),
            ChangedBy = changedBy
        };
        _context.Set<UserStatusHistory>().Add(history);

        await _context.SaveChangesAsync(ct);
        
        _logger.LogInformation("Updated user status. UserId={UserId}, OldStatus={OldStatus}, NewStatus={NewStatus}, ChangedBy={ChangedBy}",
            userId, oldStatus, newStatus, changedBy);
        
        return user;
    }

    public async Task<User> UpdateRefTierAsync(long userId, long? newParentUserId, long changedBy, string? note, CancellationToken ct = default)
    {
        var user = await GetByIdAsync(userId, ct);
        if (user == null)
            throw new InvalidOperationException($"User {userId} not found");

        // For now, we track the change in history
        // In full implementation, we'd also update a parent_user_id field if it existed
        var history = new UserRefTierHistory
        {
            UserId = userId,
            OldParentUserId = null, // Would be from user.ParentUserId if we tracked it
            NewParentUserId = newParentUserId,
            OldTier = user.Tier,
            NewTier = user.Tier, // Tier doesn't change, just the parent
            ChangedAt = _timeProvider.GetUtcNow(),
            ChangedBy = changedBy,
            Note = note
        };
        _context.Set<UserRefTierHistory>().Add(history);

        user.UpdatedAt = _timeProvider.GetUtcNow();
        user.UpdatedBy = changedBy;

        await _context.SaveChangesAsync(ct);
        
        _logger.LogInformation("Updated user ref tier. UserId={UserId}, NewParentUserId={NewParentUserId}, ChangedBy={ChangedBy}",
            userId, newParentUserId, changedBy);
        
        return user;
    }

    public async Task<bool> UserExistsWithTierAsync(long userId, string tier, CancellationToken ct = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Id == userId && u.Tier == tier, ct);
    }
}
