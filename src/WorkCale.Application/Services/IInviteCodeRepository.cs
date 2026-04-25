using WorkCale.Domain.Entities;

namespace WorkCale.Application.Services;

public interface IInviteCodeRepository
{
    /// <summary>Case-insensitive lookup by token. Returns null when not found.</summary>
    Task<InviteCode?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>Most recent first. Used by the admin list endpoint.</summary>
    Task<IReadOnlyList<InviteCode>> ListAsync(CancellationToken ct = default);

    Task AddAsync(InviteCode code, CancellationToken ct = default);
    Task UpdateAsync(InviteCode code, CancellationToken ct = default);
}
