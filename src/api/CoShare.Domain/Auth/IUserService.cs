namespace CoShare.Domain.Auth;

/// <summary>
/// Interface for USERS module integration.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Check if a phone number is already registered as an active end user.
    /// </summary>
    Task<bool> PhoneExistsAsync(string phone, CancellationToken ct = default);

    /// <summary>
    /// Check if a company exists and is active.
    /// </summary>
    Task<bool> CompanyExistsAsync(long companyId, CancellationToken ct = default);

    /// <summary>
    /// Create a new end user after successful registration.
    /// </summary>
    /// <returns>Created user ID.</returns>
    Task<long> CreateEndUserAsync(
        string phone,
        string fullName,
        long companyId,
        string correlationId,
        CancellationToken ct = default);

    /// <summary>
    /// Get user details by ID.
    /// </summary>
    Task<UserDetails?> GetUserByIdAsync(long userId, CancellationToken ct = default);
}

/// <summary>
/// User details for token generation.
/// </summary>
public sealed record UserDetails
{
    public required long UserId { get; init; }
    public required string FullName { get; init; }
    public required string Phone { get; init; }
    public required long CompanyId { get; init; }
    public required int Tier { get; init; }
}
