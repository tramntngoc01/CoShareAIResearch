namespace CoShare.Domain.Auth;

/// <summary>
/// Repository interface for refresh tokens.
/// </summary>
public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task UpdateAsync(RefreshToken token, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Get all active (non-revoked, non-expired, non-deleted) refresh tokens for a user.
    /// Used for logout operations (US-AUTH-004).
    /// </summary>
    Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(long userId, CancellationToken ct = default);
}
