namespace CoShare.Domain.Users;

/// <summary>
/// Represents one HR import execution.
/// Maps to table: users_import_job
/// Reference: DB-USERS.md, US-USERS-001
/// </summary>
public class UserImportJob
{
    public long Id { get; set; }
    
    /// <summary>
    /// External ID exposed via API
    /// </summary>
    public Guid ImportUuid { get; set; }
    
    /// <summary>
    /// Optional HR source identifier
    /// </summary>
    public string? Source { get; set; }
    
    public string FileName { get; set; } = null!;
    
    /// <summary>
    /// Status: Pending, Processing, Completed, Failed
    /// </summary>
    public string Status { get; set; } = null!;
    
    public int? TotalRows { get; set; }
    public int? CreatedRows { get; set; }
    public int? UpdatedRows { get; set; }
    public int? FailedRows { get; set; }
    
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    
    /// <summary>
    /// FK to admin_admin_user.id
    /// </summary>
    public long RequestedBy { get; set; }
    
    // Audit fields
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}
