using Microsoft.EntityFrameworkCore;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Infrastructure.Persistence.Repositories;

public class ShiftCategoryRepository(AppDbContext db) : IShiftCategoryRepository
{
    public async Task<IEnumerable<ShiftCategory>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await db.ShiftCategories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

    public Task<ShiftCategory?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.ShiftCategories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> HasShiftsAsync(Guid categoryId, CancellationToken ct = default)
        => db.Shifts.AnyAsync(s => s.CategoryId == categoryId, ct);

    public async Task AddAsync(ShiftCategory category, CancellationToken ct = default)
    {
        db.ShiftCategories.Add(category);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ShiftCategory category, CancellationToken ct = default)
    {
        db.ShiftCategories.Update(category);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ShiftCategory category, CancellationToken ct = default)
    {
        db.ShiftCategories.Remove(category);
        await db.SaveChangesAsync(ct);
    }
}
