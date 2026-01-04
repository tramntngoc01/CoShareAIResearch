namespace CoShare.Domain.Users;

/// <summary>
/// Per-row validation outcome for an import.
/// Maps to table: users_import_row
/// Reference: DB-USERS.md, US-USERS-001
/// </summary>
public class UserImportRow
{
    public long Id { get; set; }
    
    /// <summary>
    /// FK to users_import_job.id
    /// </summary>
    public long ImportJobId { get; set; }
    
    public int RowNumber { get; set; }
    
    /// <summary>
    /// Logical key, e.g. concatenation of employee code + company
    /// </summary>
    public string? LogicalKey { get; set; }
    
    /// <summary>
    /// Result: Created, Updated, Failed, Skipped
    /// </summary>
    public string Result { get; set; } = null!;
    
    public string? ErrorCode { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Raw parsed data for troubleshooting (subject to retention and masking)
    /// </summary>
    public string? RawPayload { get; set; } // Stored as JSONB in DB
    
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
}
