namespace CoShare.Domain.Common;

/// <summary>
/// Base class for all auditable entities following DB-Design-Rules.
/// </summary>
public abstract class AuditableEntity
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
