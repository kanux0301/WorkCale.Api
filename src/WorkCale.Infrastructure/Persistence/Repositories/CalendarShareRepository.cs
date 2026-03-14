using Microsoft.EntityFrameworkCore;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Infrastructure.Persistence.Repositories;

public class CalendarShareRepository(AppDbContext db) : ICalendarShareRepository
{
    public Task<CalendarShare?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.CalendarShares
            .Include(s => s.OwnerUser)
            .Include(s => s.ViewerUser)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<CalendarShare?> GetActiveShareAsync(Guid ownerUserId, Guid viewerUserId, CancellationToken ct = default)
        => db.CalendarShares
            .FirstOrDefaultAsync(s => s.OwnerUserId == ownerUserId && s.ViewerUserId == viewerUserId && s.IsActive, ct);

    public async Task<IEnumerable<CalendarShare>> GetGrantedByUserAsync(Guid ownerUserId, CancellationToken ct = default)
        => await db.CalendarShares
            .Include(s => s.ViewerUser)
            .Where(s => s.OwnerUserId == ownerUserId && s.IsActive)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<CalendarShare>> GetGrantedToUserAsync(Guid viewerUserId, CancellationToken ct = default)
        => await db.CalendarShares
            .Include(s => s.OwnerUser)
            .Where(s => s.ViewerUserId == viewerUserId && s.IsActive)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(CalendarShare share, CancellationToken ct = default)
    {
        db.CalendarShares.Add(share);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(CalendarShare share, CancellationToken ct = default)
    {
        db.CalendarShares.Update(share);
        await db.SaveChangesAsync(ct);
    }
}
