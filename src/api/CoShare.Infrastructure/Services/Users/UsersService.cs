using CoShare.Domain.Users;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CoShare.Infrastructure.Services.Users;

/// <summary>
/// Service implementation for USERS module business logic.
/// Implements US-USERS-001 through US-USERS-006.
/// </summary>
public class UsersService : IUsersService
{
    private readonly IUserRepository _userRepo;
    private readonly IUserImportRepository _importRepo;
    private readonly ILogger<UsersService> _logger;
    private readonly TimeProvider _timeProvider;

    public UsersService(
        IUserRepository userRepo,
        IUserImportRepository importRepo,
        ILogger<UsersService> logger,
        TimeProvider timeProvider)
    {
        _userRepo = userRepo;
        _importRepo = importRepo;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    // US-USERS-001: Start import job
    public async Task<(Guid importId, DateTime submittedAt)> StartImportAsync(
        string fileName,
        string? source,
        long requestedBy,
        string correlationId,
        CancellationToken ct = default)
    {
        var importUuid = Guid.NewGuid();
        var submittedAt = _timeProvider.GetUtcNow();

        var job = new UserImportJob
        {
            ImportUuid = importUuid,
            Source = source,
            FileName = fileName,
            Status = "Pending",
            RequestedBy = requestedBy,
            CreatedBy = requestedBy
        };

        await _importRepo.CreateJobAsync(job, ct);

        _logger.LogInformation(
            "Import job created. ImportUuid={ImportUuid}, FileName={FileName}, CorrelationId={CorrelationId}",
            importUuid, fileName, correlationId);

        return (importUuid, submittedAt.DateTime);
    }

    // US-USERS-001: Get import result
    public async Task<(Guid importId, string status, int? totalRows, int? created, int? updated, int? failed)> GetImportResultAsync(
        Guid importUuid,
        CancellationToken ct = default)
    {
        var job = await _importRepo.GetJobByUuidAsync(importUuid, ct);
        if (job == null)
        {
            throw new InvalidOperationException($"Import job {importUuid} not found");
        }

        return (job.ImportUuid, job.Status, job.TotalRows, job.CreatedRows, job.UpdatedRows, job.FailedRows);
    }

    // US-USERS-001: Process import (simplified - real implementation would parse CSV/Excel)
    public async Task ProcessImportAsync(Guid importUuid, Stream fileStream, string correlationId, CancellationToken ct = default)
    {
        var job = await _importRepo.GetJobByUuidAsync(importUuid, ct);
        if (job == null)
        {
            throw new InvalidOperationException($"Import job {importUuid} not found");
        }

        job.Status = "Processing";
        job.StartedAt = _timeProvider.GetUtcNow();
        await _importRepo.UpdateJobAsync(job, ct);

        try
        {
            // Simplified: In real implementation, parse CSV/Excel file here
            // For now, just mark as completed
            job.Status = "Completed";
            job.CompletedAt = _timeProvider.GetUtcNow();
            job.TotalRows = 0;
            job.CreatedRows = 0;
            job.UpdatedRows = 0;
            job.FailedRows = 0;

            await _importRepo.UpdateJobAsync(job, ct);

            _logger.LogInformation(
                "Import job completed. ImportUuid={ImportUuid}, CorrelationId={CorrelationId}",
                importUuid, correlationId);
        }
        catch (Exception ex)
        {
            job.Status = "Failed";
            job.CompletedAt = _timeProvider.GetUtcNow();
            await _importRepo.UpdateJobAsync(job, ct);

            _logger.LogError(ex,
                "Import job failed. ImportUuid={ImportUuid}, CorrelationId={CorrelationId}",
                importUuid, correlationId);
            throw;
        }
    }

    // US-USERS-002: Get ref tier
    public async Task<(long userId, string tier, long? parentUserId, string? parentName, string? parentTier)> GetRefTierAsync(
        long userId,
        CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        // Simplified: In real implementation, query parent user from ref tier history
        return (user.Id, user.Tier, null, null, null);
    }

    // US-USERS-002: Change ref tier
    public async Task<(long userId, string tier, long? parentUserId, string? parentName, string? parentTier)> ChangeRefTierAsync(
        long userId,
        long newParentUserId,
        long changedBy,
        string? note,
        string correlationId,
        CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        var parentUser = await _userRepo.GetByIdAsync(newParentUserId, ct);
        if (parentUser == null)
        {
            throw new InvalidOperationException($"Parent user {newParentUserId} not found");
        }

        // BR-010: Validate ref tier hierarchy
        var validParentTier = user.Tier switch
        {
            "T3" => "T2",
            "T2" => "T1",
            _ => null
        };

        if (validParentTier != null && parentUser.Tier != validParentTier)
        {
            throw new InvalidOperationException(
                $"Invalid parent tier. User tier {user.Tier} requires parent tier {validParentTier}, but got {parentUser.Tier}");
        }

        await _userRepo.UpdateRefTierAsync(userId, newParentUserId, changedBy, note, ct);

        _logger.LogInformation(
            "Ref tier changed. UserId={UserId}, NewParentUserId={NewParentUserId}, ChangedBy={ChangedBy}, CorrelationId={CorrelationId}",
            userId, newParentUserId, changedBy, correlationId);

        return (user.Id, user.Tier, parentUser.Id, parentUser.FullName, parentUser.Tier);
    }

    // US-USERS-003: Get user by ID
    public async Task<User?> GetUserByIdAsync(long userId, CancellationToken ct = default)
    {
        return await _userRepo.GetByIdAsync(userId, ct);
    }

    // US-USERS-003: Update KYC
    public async Task<User> UpdateKycAsync(
        long userId,
        string? fullName,
        string? email,
        string? addressDetail,
        DateTime? birthDate,
        long updatedBy,
        string correlationId,
        CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        // Update only allowed fields per BR-011
        if (!string.IsNullOrEmpty(fullName))
            user.FullName = fullName;
        if (email != null)
            user.Email = email;
        if (addressDetail != null)
            user.AddressDetail = addressDetail;
        if (birthDate != null)
            user.BirthDate = birthDate;

        user.UpdatedBy = updatedBy;

        await _userRepo.UpdateAsync(user, ct);

        _logger.LogInformation(
            "User KYC updated. UserId={UserId}, UpdatedBy={UpdatedBy}, CorrelationId={CorrelationId}",
            userId, updatedBy, correlationId);

        return user;
    }

    // US-USERS-005: Change status
    public async Task<User> ChangeStatusAsync(
        long userId,
        string newStatus,
        string? reason,
        long changedBy,
        string correlationId,
        CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        // Validate status transition
        var validStatuses = new[] { "Draft", "Active", "Inactive", "Locked" };
        if (!validStatuses.Contains(newStatus))
        {
            throw new InvalidOperationException($"Invalid status: {newStatus}");
        }

        if (user.Status == newStatus)
        {
            // No-op if status is already set
            _logger.LogInformation(
                "Status unchanged. UserId={UserId}, Status={Status}, CorrelationId={CorrelationId}",
                userId, newStatus, correlationId);
            return user;
        }

        await _userRepo.UpdateStatusAsync(userId, newStatus, changedBy, reason, ct);

        _logger.LogInformation(
            "User status changed. UserId={UserId}, OldStatus={OldStatus}, NewStatus={NewStatus}, ChangedBy={ChangedBy}, CorrelationId={CorrelationId}",
            userId, user.Status, newStatus, changedBy, correlationId);

        user.Status = newStatus;
        return user;
    }

    // US-USERS-006: Search users
    public async Task<(List<User> items, int totalCount)> SearchUsersAsync(
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
        // Validate pagination
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        return await _userRepo.SearchAsync(
            companyId,
            pickupPointId,
            tier,
            status,
            employeeCode,
            phone,
            page,
            pageSize,
            ct);
    }
}
