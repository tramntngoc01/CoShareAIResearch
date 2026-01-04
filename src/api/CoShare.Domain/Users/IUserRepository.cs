using CoShare.Domain.Users;

namespace CoShare.Domain.Users;

/// <summary>
/// Repository interface for User entity operations.
/// Reference: DB-USERS.md, US-USERS-001 through US-USERS-006
/// </summary>
public interface IUserRepository
{
    // Query operations
    Task<User?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<User?> GetByEmployeeCodeAndCompanyAsync(string employeeCode, long companyId, CancellationToken ct = default);
    Task<User?> GetByPhoneAndCompanyAsync(string phone, long companyId, CancellationToken ct = default);
    
    // Search/List with filters - US-USERS-006
    Task<(List<User> items, int totalCount)> SearchAsync(
        long? companyId,
        long? pickupPointId,
        string? tier,
        string? status,
        string? employeeCode,
        string? phone,
        int page,
        int pageSize,
        CancellationToken ct = default);
    
    // Create/Update operations
    Task<User> CreateAsync(User user, CancellationToken ct = default);
    Task<User> UpdateAsync(User user, CancellationToken ct = default);
    
    // Status management - US-USERS-005
    Task<User> UpdateStatusAsync(long userId, string newStatus, long changedBy, string? reason, CancellationToken ct = default);
    
    // Ref Tier management - US-USERS-002
    Task<User> UpdateRefTierAsync(long userId, long? newParentUserId, long changedBy, string? note, CancellationToken ct = default);
    
    // Check if parent exists and has correct tier - US-USERS-002
    Task<bool> UserExistsWithTierAsync(long userId, string tier, CancellationToken ct = default);
}
