using WorkCale.Domain.Entities;

namespace WorkCale.Application.Services;

public interface IJobRepository
{
    Task<IEnumerable<Job>> GetByUserIdAsync(Guid userId, bool includeArchived = false, CancellationToken ct = default);
    Task<Job?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Job?> GetDefaultAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasShiftsAsync(Guid jobId, CancellationToken ct = default);
    Task<bool> HasCategoriesAsync(Guid jobId, CancellationToken ct = default);
    Task AddAsync(Job job, CancellationToken ct = default);
    Task UpdateAsync(Job job, CancellationToken ct = default);
    Task DeleteAsync(Job job, CancellationToken ct = default);
    /// <summary>Atomically switch the default flag: clear the current default and set the new one.</summary>
    Task SwapDefaultAsync(Guid userId, Guid newDefaultJobId, CancellationToken ct = default);
}
