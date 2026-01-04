namespace CoShare.Domain.Auth;

/// <summary>
/// Admin user details for authentication (US-AUTH-005).
/// </summary>
public sealed record AdminUserDetails
{
    public required long UserId { get; init; }
    public required string LoginId { get; init; }
    public required string PasswordHash { get; init; }
    public required string Role { get; init; }
    public required long CompanyId { get; init; }
    public required bool IsActive { get; init; }
    public required bool IsLocked { get; init; }
    public required bool IsDeleted { get; init; }
}
