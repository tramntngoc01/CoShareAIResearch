using CoShare.Domain.Auth;
using CoShare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoShare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for refresh tokens.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _context;

    public RefreshTokenRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        await _context.RefreshTokens.AddAsync(token, ct);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
    }

    public Task UpdateAsync(RefreshToken token, CancellationToken ct = default)
    {
        _context.RefreshTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
