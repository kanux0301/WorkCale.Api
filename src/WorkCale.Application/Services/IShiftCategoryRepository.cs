using WorkCale.Domain.Entities;

namespace WorkCale.Application.Services;

public interface IShiftCategoryRepository
{
    Task<IEnumerable<ShiftCategory>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<ShiftCategory?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasShiftsAsync(Guid categoryId, CancellationToken ct = default);
    Task AddAsync(ShiftCategory category, CancellationToken ct = default);
    Task UpdateAsync(ShiftCategory category, CancellationToken ct = default);
    Task DeleteAsync(ShiftCategory category, CancellationToken ct = default);
}
