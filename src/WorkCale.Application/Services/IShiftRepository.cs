using WorkCale.Domain.Entities;

namespace WorkCale.Application.Services;

public interface IShiftRepository
{
    Task<IEnumerable<Shift>> GetByUserAndMonthAsync(Guid userId, int year, int month, CancellationToken ct = default);
    Task<Shift?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Shift shift, CancellationToken ct = default);
    Task UpdateAsync(Shift shift, CancellationToken ct = default);
    Task DeleteAsync(Shift shift, CancellationToken ct = default);
}
