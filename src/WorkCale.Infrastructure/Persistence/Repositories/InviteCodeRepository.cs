using Microsoft.EntityFrameworkCore;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Infrastructure.Persistence.Repositories;

public class InviteCodeRepository(AppDbContext db) : IInviteCodeRepository
{
    public Task<InviteCode?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        // Codes are stored upper-cased; normalise the lookup so users can type either case.
        var upper = code.Trim().ToUpperInvariant();
        return db.Set<InviteCode>().FirstOrDefaultAsync(c => c.Code == upper, ct);
    }

    public async Task<IReadOnlyList<InviteCode>> ListAsync(CancellationToken ct = default)
    {
        return await db.Set<InviteCode>()
            .OrderByDescending(c => c.IssuedAt)
            .Take(200)
            .ToListAsync(ct);
    }

    public async Task AddAsync(InviteCode code, CancellationToken ct = default)
    {
        db.Set<InviteCode>().Add(code);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(InviteCode code, CancellationToken ct = default)
    {
        db.Set<InviteCode>().Update(code);
        await db.SaveChangesAsync(ct);
    }
}
