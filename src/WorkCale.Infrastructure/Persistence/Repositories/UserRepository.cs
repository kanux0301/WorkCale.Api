using Microsoft.EntityFrameworkCore;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default)
        => db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId, ct);

    public async Task<IEnumerable<User>> SearchAsync(string query, CancellationToken ct = default)
    {
        var lower = query.ToLowerInvariant();
        return await db.Users
            .Where(u => u.Email.Contains(lower) || u.DisplayName.ToLower().Contains(lower))
            .Take(20)
            .ToListAsync(ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
    }
}
