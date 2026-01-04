using CoShare.Domain.Users;

namespace CoShare.Domain.Users;

/// <summary>
/// Service interface for USERS module business logic.
/// Implements US-USERS-001 through US-USERS-006.
/// </summary>
public interface IUsersService
{
    // US-USERS-001: Import users from HR file
    Task<(Guid importId, DateTime submittedAt)> StartImportAsync(
        string fileName,
        string? source,
        long requestedBy,
        string correlationId,
        CancellationToken ct = default);
    
    Task<(Guid importId, string status, int? totalRows, int? created, int? updated, int? failed)> GetImportResultAsync(
        Guid importUuid,
        CancellationToken ct = default);
    
    Task ProcessImportAsync(Guid importUuid, Stream fileStream, string correlationId, CancellationToken ct = default);
    
    // US-USERS-002: Manage Ref Tier
    Task<(long userId, string tier, long? parentUserId, string? parentName, string? parentTier)> GetRefTierAsync(
        long userId,
        CancellationToken ct = default);
    
    Task<(long userId, string tier, long? parentUserId, string? parentName, string? parentTier)> ChangeRefTierAsync(
        long userId,
        long newParentUserId,
        long changedBy,
        string? note,
        string correlationId,
        CancellationToken ct = default);
    
    // US-USERS-003: Admin KYC/profile updates
    Task<User?> GetUserByIdAsync(long userId, CancellationToken ct = default);
    
    Task<User> UpdateKycAsync(
        long userId,
        string? fullName,
        string? email,
        string? addressDetail,
        DateTime? birthDate,
        long updatedBy,
        string correlationId,
        CancellationToken ct = default);
    
    // US-USERS-005: Status management
    Task<User> ChangeStatusAsync(
        long userId,
        string newStatus,
        string? reason,
        long changedBy,
        string correlationId,
        CancellationToken ct = default);
    
    // US-USERS-006: Search/list users
    Task<(List<User> items, int totalCount)> SearchUsersAsync(
        long? companyId,
        long? pickupPointId,
        string? tier,
        string? status,
        string? employeeCode,
        string? phone,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
