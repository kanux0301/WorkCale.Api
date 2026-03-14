using WorkCale.Domain.Entities;

namespace WorkCale.Application.Services;

public interface ICalendarShareRepository
{
    Task<CalendarShare?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CalendarShare?> GetActiveShareAsync(Guid ownerUserId, Guid viewerUserId, CancellationToken ct = default);
    Task<IEnumerable<CalendarShare>> GetGrantedByUserAsync(Guid ownerUserId, CancellationToken ct = default);
    Task<IEnumerable<CalendarShare>> GetGrantedToUserAsync(Guid viewerUserId, CancellationToken ct = default);
    Task AddAsync(CalendarShare share, CancellationToken ct = default);
    Task UpdateAsync(CalendarShare share, CancellationToken ct = default);
}
