namespace CoShare.Api.Contracts.Users;

/// <summary>
/// Response for import job creation - US-USERS-001
/// </summary>
public record UsersImportJobCreated
{
    public Guid ImportId { get; init; }
    public DateTime SubmittedAt { get; init; }
}

/// <summary>
/// Response for import job result - US-USERS-001
/// </summary>
public record UsersImportJobResult
{
    public Guid ImportId { get; init; }
    public string Status { get; init; } = null!;
    public int? TotalRows { get; init; }
    public int? Created { get; init; }
    public int? Updated { get; init; }
    public int? Failed { get; init; }
    public List<ErrorSample>? ErrorSamples { get; init; }
}

public record ErrorSample
{
    public int RowNumber { get; init; }
    public string ErrorCode { get; init; } = null!;
    public string Message { get; init; } = null!;
}

/// <summary>
/// User summary for list/search - US-USERS-006
/// </summary>
public record UserSummary
{
    public long Id { get; init; }
    public string FullName { get; init; } = null!;
    public string Phone { get; init; } = null!;
    public string EmployeeCode { get; init; } = null!;
    public long CompanyId { get; init; }
    public string? CompanyName { get; init; }
    public long PickupPointId { get; init; }
    public string? PickupPointName { get; init; }
    public string Tier { get; init; } = null!;
    public string Status { get; init; } = null!;
}

/// <summary>
/// User detail with all fields - US-USERS-003, US-USERS-004, US-USERS-005
/// </summary>
public record UserDetail : UserSummary
{
    public string? CccdMasked { get; init; }
    public DateTime? BirthDate { get; init; }
    public string? Email { get; init; }
    public string? AddressDetail { get; init; }
    public Dictionary<string, string>? KycMetadata { get; init; }
}

/// <summary>
/// Request to update KYC fields - US-USERS-003
/// </summary>
public record UserKycUpdateRequest
{
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public string? AddressDetail { get; init; }
    public DateTime? BirthDate { get; init; }
}

/// <summary>
/// Request to change user status - US-USERS-005
/// </summary>
public record UserStatusChangeRequest
{
    public string NewStatus { get; init; } = null!;
    public string? Reason { get; init; }
}

/// <summary>
/// Ref tier information - US-USERS-002
/// </summary>
public record UserRefTierInfo
{
    public long UserId { get; init; }
    public string Tier { get; init; } = null!;
    public long? ParentUserId { get; init; }
    public string? ParentUserName { get; init; }
    public string? ParentTier { get; init; }
}

/// <summary>
/// Request to change ref tier - US-USERS-002
/// </summary>
public record UserRefTierChangeRequest
{
    public long NewParentUserId { get; init; }
    public string? Note { get; init; }
}

/// <summary>
/// Paged result wrapper
/// </summary>
public record PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
}
