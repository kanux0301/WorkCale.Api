using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WorkCale.Domain.Entities;
using WorkCale.Infrastructure.Persistence;
using WorkCale.Infrastructure.Persistence.Repositories;

namespace WorkCale.Infrastructure.Tests.Persistence;

public class ShiftRepositoryTests
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

    private static async Task<(User user, ShiftCategory cat)> SeedAsync(AppDbContext db)
    {
        var user = User.Create("u@test.com", "User", "hash");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var cat = ShiftCategory.Create(user.Id, "Day", "#F59E0B");
        db.ShiftCategories.Add(cat);
        await db.SaveChangesAsync();

        return (user, cat);
    }

    [Fact]
    public async Task AddAsync_PersistsShift()
    {
        await using var db = CreateDb();
        var (user, cat) = await SeedAsync(db);
        var repo = new ShiftRepository(db);

        var shift = Shift.Create(user.Id, cat.Id, new DateOnly(2026, 3, 10),
            new TimeOnly(9, 0), new TimeOnly(17, 0), null, null);
        await repo.AddAsync(shift);

        db.Shifts.Should().ContainSingle();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsShift_WithCategory()
    {
        await using var db = CreateDb();
        var (user, cat) = await SeedAsync(db);
        var repo = new ShiftRepository(db);

        var shift = Shift.Create(user.Id, cat.Id, new DateOnly(2026, 3, 10),
            new TimeOnly(9, 0), new TimeOnly(17, 0), "Office", null);
        await repo.AddAsync(shift);

        var found = await repo.GetByIdAsync(shift.Id);

        found.Should().NotBeNull();
        found!.Location.Should().Be("Office");
        found.Category.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        await using var db = CreateDb();
        var repo = new ShiftRepository(db);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserAndMonthAsync_ReturnsShiftsInMonth()
    {
        await using var db = CreateDb();
        var (user, cat) = await SeedAsync(db);
        var repo = new ShiftRepository(db);

        // In March 2026
        var s1 = Shift.Create(user.Id, cat.Id, new DateOnly(2026, 3, 5),
            new TimeOnly(9, 0), new TimeOnly(17, 0), null, null);
        var s2 = Shift.Create(user.Id, cat.Id, new DateOnly(2026, 3, 20),
            new TimeOnly(9, 0), new TimeOnly(17, 0), null, null);
        // Outside March 2026
        var s3 = Shift.Create(user.Id, cat.Id, new DateOnly(2026, 4, 1),
            new TimeOnly(9, 0), new TimeOnly(17, 0), null, null);

        await repo.AddAsync(s1);
        await repo.AddAsync(s2);
        await repo.AddAsync(s3);

        var results = (await repo.GetByUserAndMonthAsync(user.Id, 2026, 3)).ToList();

        results.Should().HaveCount(2);
        results.Should().OnlyContain(s => s.Date.Month == 3 && s.Date.Year == 2026);
    }

    [Fact]
    public async Task GetByUserAndMonthAsync_ReturnsEmpty_WhenNoShifts()
    {
        await using var db = CreateDb();
        var (user, _) = await SeedAsync(db);
        var repo = new ShiftRepository(db);

        var results = await repo.GetByUserAndMonthAsync(user.Id, 2026, 1);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserAndMonthAsync_OnlyReturnsShiftsForRequestedUser()
    {
        await using var db = CreateDb();
        var (user, cat) = await SeedAsync(db);

        var otherUser = User.Create("other@test.com", "Other", "hash");
        db.Users.Add(otherUser);
        await db.SaveChangesAsync();

        var repo = new ShiftRepository(db);

        await repo.AddAsync(Shift.Create(user.Id, cat.Id, new DateOnly(2026, 3, 1),
            new TimeOnly(9, 0), new TimeOnly(17, 0), null, null));
        await repo.AddAsync(Shift.Create(otherUser.Id, cat.Id, new DateOnly(2026, 3, 2),
            new TimeOnly(9, 0), new TimeOnly(17, 0), null, null));

        var results = (await repo.GetByUserAndMonthAsync(user.Id, 2026, 3)).ToList();

        results.Should().ContainSingle();
        results[0].UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        await using var db = CreateDb();
        var (user, cat) = await SeedAsync(db);
        var repo = new ShiftRepository(db);

        var shift = Shift.Create(user.Id, cat.Id, new DateOnly(2026, 3, 10),
            new TimeOnly(9, 0), new TimeOnly(17, 0), "Office", null);
        await repo.AddAsync(shift);

        shift.Update(cat.Id, new DateOnly(2026, 3, 11), new TimeOnly(8, 0), new TimeOnly(16, 0), "Home", "Notes");
        await repo.UpdateAsync(shift);

        var updated = await repo.GetByIdAsync(shift.Id);
        updated!.Location.Should().Be("Home");
        updated.Notes.Should().Be("Notes");
    }

    [Fact]
    public async Task DeleteAsync_RemovesShift()
    {
        await using var db = CreateDb();
        var (user, cat) = await SeedAsync(db);
        var repo = new ShiftRepository(db);

        var shift = Shift.Create(user.Id, cat.Id, new DateOnly(2026, 3, 10),
            new TimeOnly(9, 0), new TimeOnly(17, 0), null, null);
        await repo.AddAsync(shift);

        await repo.DeleteAsync(shift);

        (await repo.GetByIdAsync(shift.Id)).Should().BeNull();
    }
}
