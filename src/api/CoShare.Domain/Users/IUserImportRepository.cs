using CoShare.Domain.Users;

namespace CoShare.Domain.Users;

/// <summary>
/// Repository interface for UserImportJob operations.
/// Reference: DB-USERS.md, US-USERS-001
/// </summary>
public interface IUserImportRepository
{
    // Import job operations
    Task<UserImportJob> CreateJobAsync(UserImportJob job, CancellationToken ct = default);
    Task<UserImportJob?> GetJobByUuidAsync(Guid importUuid, CancellationToken ct = default);
    Task<UserImportJob> UpdateJobAsync(UserImportJob job, CancellationToken ct = default);
    
    // Import row operations
    Task AddRowsAsync(List<UserImportRow> rows, CancellationToken ct = default);
    Task<List<UserImportRow>> GetRowsByJobIdAsync(long jobId, CancellationToken ct = default);
    
    // Get error samples for API response
    Task<List<UserImportRow>> GetErrorSamplesAsync(long jobId, int maxSamples, CancellationToken ct = default);
}
