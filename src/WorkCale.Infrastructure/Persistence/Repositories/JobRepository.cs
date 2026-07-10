using Microsoft.EntityFrameworkCore;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Infrastructure.Persistence.Repositories;

public class JobRepository(AppDbContext db) : IJobRepository
{
    public async Task<IEnumerable<Job>> GetByUserIdAsync(Guid userId, bool includeArchived = false, CancellationToken ct = default)
    {
        var query = db.Jobs.Where(j => j.UserId == userId);
        if (!includeArchived)
            query = query.Where(j => !j.IsArchived);
        return await query
            .OrderByDescending(j => j.IsDefault)
            .ThenBy(j => j.SortOrder)
            .ThenBy(j => j.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<Job?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Jobs.FirstOrDefaultAsync(j => j.Id == id, ct);

    public Task<Job?> GetDefaultAsync(Guid userId, CancellationToken ct = default)
        => db.Jobs.FirstOrDefaultAsync(j => j.UserId == userId && j.IsDefault, ct);

    public Task<bool> HasShiftsAsync(Guid jobId, CancellationToken ct = default)
        => db.Shifts.AnyAsync(s => s.JobId == jobId, ct);

    public Task<bool> HasCategoriesAsync(Guid jobId, CancellationToken ct = default)
        => db.ShiftCategories.AnyAsync(c => c.JobId == jobId, ct);

    public async Task AddAsync(Job job, CancellationToken ct = default)
    {
        db.Jobs.Add(job);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Job job, CancellationToken ct = default)
    {
        db.Jobs.Update(job);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Job job, CancellationToken ct = default)
    {
        db.Jobs.Remove(job);
        await db.SaveChangesAsync(ct);
    }

    public async Task SwapDefaultAsync(Guid userId, Guid newDefaultJobId, CancellationToken ct = default)
    {
        var jobs = await db.Jobs.Where(j => j.UserId == userId).ToListAsync(ct);
        foreach (var j in jobs)
        {
            if (j.Id == newDefaultJobId && !j.IsDefault) j.MakeDefault();
            else if (j.Id != newDefaultJobId && j.IsDefault) j.ClearDefault();
        }
        await db.SaveChangesAsync(ct);
    }
}
