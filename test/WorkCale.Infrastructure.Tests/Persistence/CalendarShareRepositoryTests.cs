using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WorkCale.Domain.Entities;
using WorkCale.Infrastructure.Persistence;
using WorkCale.Infrastructure.Persistence.Repositories;

namespace WorkCale.Infrastructure.Tests.Persistence;

public class CalendarShareRepositoryTests
{
    private static AppDbContext CreateDb()
    {
        var efServices = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseInternalServiceProvider(efServices)
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<(User owner, User viewer)> SeedUsersAsync(AppDbContext db)
    {
        var owner = User.Create("owner@test.com", "Owner", "hash");
        var viewer = User.Create("viewer@test.com", "Viewer", "hash");
        db.Users.AddRange(owner, viewer);
        await db.SaveChangesAsync();
        return (owner, viewer);
    }

    [Fact]
    public async Task AddAsync_PersistsShare()
    {
        await using var db = CreateDb();
        var (owner, viewer) = await SeedUsersAsync(db);
        var repo = new CalendarShareRepository(db);
        var share = CalendarShare.Create(owner.Id, viewer.Id);

        await repo.AddAsync(share);

        db.CalendarShares.Should().ContainSingle();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsShare_WithNavProperties()
    {
        await using var db = CreateDb();
        var (owner, viewer) = await SeedUsersAsync(db);
        var repo = new CalendarShareRepository(db);
        var share = CalendarShare.Create(owner.Id, viewer.Id);
        await repo.AddAsync(share);

        var found = await repo.GetByIdAsync(share.Id);

        found.Should().NotBeNull();
        found!.OwnerUser.Should().NotBeNull();
        found.ViewerUser.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        await using var db = CreateDb();
        var repo = new CalendarShareRepository(db);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveShareAsync_ReturnsShare_WhenActiveExists()
    {
        await using var db = CreateDb();
        var (owner, viewer) = await SeedUsersAsync(db);
        var repo = new CalendarShareRepository(db);
        var share = CalendarShare.Create(owner.Id, viewer.Id);
        await repo.AddAsync(share);

        var found = await repo.GetActiveShareAsync(owner.Id, viewer.Id);

        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetActiveShareAsync_ReturnsNull_WhenRevoked()
    {
        await using var db = CreateDb();
        var (owner, viewer) = await SeedUsersAsync(db);
        var repo = new CalendarShareRepository(db);
        var share = CalendarShare.Create(owner.Id, viewer.Id);
        await repo.AddAsync(share);
        share.Revoke();
        await repo.UpdateAsync(share);

        var found = await repo.GetActiveShareAsync(owner.Id, viewer.Id);

        found.Should().BeNull();
    }

    [Fact]
    public async Task GetGrantedByUserAsync_ReturnsActiveSharesForOwner()
    {
        await using var db = CreateDb();
        var (owner, viewer) = await SeedUsersAsync(db);
        var viewer2 = User.Create("v2@test.com", "V2", "hash");
        db.Users.Add(viewer2);
        await db.SaveChangesAsync();

        var repo = new CalendarShareRepository(db);
        var share1 = CalendarShare.Create(owner.Id, viewer.Id);
        var share2 = CalendarShare.Create(owner.Id, viewer2.Id);
        await repo.AddAsync(share1);
        await repo.AddAsync(share2);
        share2.Revoke();
        await repo.UpdateAsync(share2);

        var results = (await repo.GetGrantedByUserAsync(owner.Id)).ToList();

        results.Should().ContainSingle();
        results[0].ViewerUserId.Should().Be(viewer.Id);
    }

    [Fact]
    public async Task GetGrantedToUserAsync_ReturnsActiveSharesForViewer()
    {
        await using var db = CreateDb();
        var (owner, viewer) = await SeedUsersAsync(db);
        var repo = new CalendarShareRepository(db);
        var share = CalendarShare.Create(owner.Id, viewer.Id);
        await repo.AddAsync(share);

        var results = (await repo.GetGrantedToUserAsync(viewer.Id)).ToList();

        results.Should().ContainSingle();
        results[0].OwnerUserId.Should().Be(owner.Id);
    }

    [Fact]
    public async Task UpdateAsync_PersistsRevokedState()
    {
        await using var db = CreateDb();
        var (owner, viewer) = await SeedUsersAsync(db);
        var repo = new CalendarShareRepository(db);
        var share = CalendarShare.Create(owner.Id, viewer.Id);
        await repo.AddAsync(share);

        share.Revoke();
        await repo.UpdateAsync(share);

        var updated = await repo.GetByIdAsync(share.Id);
        updated!.IsActive.Should().BeFalse();
        updated.RevokedAt.Should().NotBeNull();
    }
}
