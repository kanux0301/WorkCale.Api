using Microsoft.EntityFrameworkCore;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(RefreshToken token, CancellationToken ct = default)
    {
        db.RefreshTokens.Remove(token);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await db.RefreshTokens.Where(r => r.UserId == userId).ToListAsync(ct);
        db.RefreshTokens.RemoveRange(tokens);
        await db.SaveChangesAsync(ct);
    }
}
