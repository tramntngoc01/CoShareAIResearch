namespace CoShare.Domain.Users;

/// <summary>
/// History of ref tier (parent) changes for a user.
/// Maps to table: users_ref_tier_history
/// Reference: DB-USERS.md, US-USERS-002
/// </summary>
public class UserRefTierHistory
{
    public long Id { get; set; }
    
    /// <summary>
    /// FK to users_user.id
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// FK to users_user.id (nullable - old parent)
    /// </summary>
    public long? OldParentUserId { get; set; }
    
    /// <summary>
    /// FK to users_user.id (nullable - new parent)
    /// </summary>
    public long? NewParentUserId { get; set; }
    
    public string OldTier { get; set; } = null!;
    
    public string NewTier { get; set; } = null!;
    
    public DateTimeOffset ChangedAt { get; set; }
    
    /// <summary>
    /// FK to admin_admin_user.id (actor in Admin portal)
    /// </summary>
    public long ChangedBy { get; set; }
    
    public string? Note { get; set; }
}
