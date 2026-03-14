using Microsoft.EntityFrameworkCore;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Infrastructure.Persistence.Repositories;

public class ShiftRepository(AppDbContext db) : IShiftRepository
{
    public async Task<IEnumerable<Shift>> GetByUserAndMonthAsync(Guid userId, int year, int month, CancellationToken ct = default)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        return await db.Shifts
            .Include(s => s.Category)
            .Where(s => s.UserId == userId && s.Date >= start && s.Date <= end)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .ToListAsync(ct);
    }

    public Task<Shift?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Shifts.Include(s => s.Category).FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddAsync(Shift shift, CancellationToken ct = default)
    {
        db.Shifts.Add(shift);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Shift shift, CancellationToken ct = default)
    {
        db.Shifts.Update(shift);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Shift shift, CancellationToken ct = default)
    {
        db.Shifts.Remove(shift);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateTimesByCategoryAsync(Guid categoryId, TimeOnly startTime, TimeOnly endTime, CancellationToken ct = default)
    {
        await db.Shifts
            .Where(s => s.CategoryId == categoryId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.StartTime, startTime)
                .SetProperty(x => x.EndTime, endTime)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);
    }
}
