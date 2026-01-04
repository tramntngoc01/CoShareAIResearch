using CoShare.Domain.Users;
using CoShare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoShare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for User import operations.
/// Reference: DB-USERS.md, US-USERS-001
/// </summary>
public class UserImportRepository : IUserImportRepository
{
    private readonly UsersDbContext _context;
    private readonly ILogger<UserImportRepository> _logger;
    private readonly TimeProvider _timeProvider;

    public UserImportRepository(
        UsersDbContext context,
        ILogger<UserImportRepository> logger,
        TimeProvider timeProvider)
    {
        _context = context;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<UserImportJob> CreateJobAsync(UserImportJob job, CancellationToken ct = default)
    {
        job.CreatedAt = _timeProvider.GetUtcNow();
        _context.ImportJobs.Add(job);
        await _context.SaveChangesAsync(ct);
        
        _logger.LogInformation("Created import job. JobId={JobId}, ImportUuid={ImportUuid}, FileName={FileName}",
            job.Id, job.ImportUuid, job.FileName);
        
        return job;
    }

    public async Task<UserImportJob?> GetJobByUuidAsync(Guid importUuid, CancellationToken ct = default)
    {
        return await _context.ImportJobs
            .FirstOrDefaultAsync(j => j.ImportUuid == importUuid, ct);
    }

    public async Task<UserImportJob> UpdateJobAsync(UserImportJob job, CancellationToken ct = default)
    {
        job.UpdatedAt = _timeProvider.GetUtcNow();
        _context.ImportJobs.Update(job);
        await _context.SaveChangesAsync(ct);
        
        _logger.LogInformation("Updated import job. JobId={JobId}, ImportUuid={ImportUuid}, Status={Status}",
            job.Id, job.ImportUuid, job.Status);
        
        return job;
    }

    public async Task AddRowsAsync(List<UserImportRow> rows, CancellationToken ct = default)
    {
        var now = _timeProvider.GetUtcNow();
        foreach (var row in rows)
        {
            row.CreatedAt = now;
        }
        
        _context.ImportRows.AddRange(rows);
        await _context.SaveChangesAsync(ct);
        
        _logger.LogInformation("Added {Count} import rows", rows.Count);
    }

    public async Task<List<UserImportRow>> GetRowsByJobIdAsync(long jobId, CancellationToken ct = default)
    {
        return await _context.ImportRows
            .Where(r => r.ImportJobId == jobId)
            .OrderBy(r => r.RowNumber)
            .ToListAsync(ct);
    }

    public async Task<List<UserImportRow>> GetErrorSamplesAsync(long jobId, int maxSamples, CancellationToken ct = default)
    {
        return await _context.ImportRows
            .Where(r => r.ImportJobId == jobId && r.Result == "Failed")
            .OrderBy(r => r.RowNumber)
            .Take(maxSamples)
            .ToListAsync(ct);
    }
}
