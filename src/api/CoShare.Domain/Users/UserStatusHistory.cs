namespace CoShare.Domain.Users;

/// <summary>
/// History of status changes for a user.
/// Maps to table: users_status_history
/// Reference: DB-USERS.md, US-USERS-005
/// </summary>
public class UserStatusHistory
{
    public long Id { get; set; }
    
    /// <summary>
    /// FK to users_user.id
    /// </summary>
    public long UserId { get; set; }
    
    public string OldStatus { get; set; } = null!;
    
    public string NewStatus { get; set; } = null!;
    
    public string? Reason { get; set; }
    
    public DateTimeOffset ChangedAt { get; set; }
    
    /// <summary>
    /// FK to admin_admin_user.id
    /// </summary>
    public long ChangedBy { get; set; }
}
