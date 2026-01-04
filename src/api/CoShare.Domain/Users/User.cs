namespace CoShare.Domain.Users;

/// <summary>
/// User entity - core user master data (shippers, tiers 1/2/3).
/// Maps to table: users_user
/// Reference: DB-USERS.md, US-USERS-001 through US-USERS-006
/// </summary>
public class User
{
    public long Id { get; set; }
    
    /// <summary>
    /// Employee code from HR; together with CompanyId forms a logical key.
    /// </summary>
    public string ExternalEmployeeCode { get; set; } = null!;
    
    public string FullName { get; set; } = null!;
    
    public string Phone { get; set; } = null!;
    
    /// <summary>
    /// FK to admin_company.id (ADMIN module)
    /// </summary>
    public long CompanyId { get; set; }
    
    /// <summary>
    /// FK to admin_pickup_point.id (ADMIN module)
    /// </summary>
    public long PickupPointId { get; set; }
    
    /// <summary>
    /// Tier: T1, T2, T3, SHIPPER
    /// </summary>
    public string Tier { get; set; } = null!;
    
    /// <summary>
    /// Logical status: Draft, Active, Inactive, Locked, Deleted
    /// </summary>
    public string Status { get; set; } = null!;
    
    public string? Email { get; set; }
    
    public string? AddressDetail { get; set; }
    
    public DateTime? BirthDate { get; set; }
    
    /// <summary>
    /// Hashed/tokenized CCCD; raw value never stored.
    /// </summary>
    public string? CccdHash { get; set; }
    
    /// <summary>
    /// Extensible key/value bag for additional KYC fields.
    /// </summary>
    public string? KycMetadata { get; set; } // Stored as JSONB in DB
    
    // Audit fields
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}
