using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WorkCale.Domain.Entities;
using WorkCale.Infrastructure.Persistence;
using WorkCale.Infrastructure.Persistence.Repositories;

namespace WorkCale.Infrastructure.Tests.Persistence;

public class ShiftCategoryRepositoryTests
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

    private static async Task<User> SeedUserAsync(AppDbContext db)
    {
        var user = User.Create("u@test.com", "User", "hash");
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task AddAsync_PersistsCategory()
    {
        await using var db = CreateDb();
        var user = await SeedUserAsync(db);
        var repo = new ShiftCategoryRepository(db);
        var cat = ShiftCategory.Create(user.Id, "Day Shift", "#F59E0B");

        await repo.AddAsync(cat);

        db.ShiftCategories.Should().ContainSingle(c => c.Name == "Day Shift");
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsUserCategories()
    {
        await using var db = CreateDb();
        var user = await SeedUserAsync(db);
        var otherUser = User.Create("other@test.com", "Other", "hash");
        db.Users.Add(otherUser);
        await db.SaveChangesAsync();

        var repo = new ShiftCategoryRepository(db);
        await repo.AddAsync(ShiftCategory.Create(user.Id, "Cat1", "#AABBCC"));
        await repo.AddAsync(ShiftCategory.Create(user.Id, "Cat2", "#112233"));
        await repo.AddAsync(ShiftCategory.Create(otherUser.Id, "Other Cat", "#445566"));

        var results = (await repo.GetByUserIdAsync(user.Id)).ToList();

        results.Should().HaveCount(2);
        results.Should().OnlyContain(c => c.UserId == user.Id);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsEmpty_WhenNoCategories()
    {
        await using var db = CreateDb();
        var user = await SeedUserAsync(db);
        var repo = new ShiftCategoryRepository(db);

        var results = await repo.GetByUserIdAsync(user.Id);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCategory_WhenExists()
    {
        await using var db = CreateDb();
        var user = await SeedUserAsync(db);
        var repo = new ShiftCategoryRepository(db);
        var cat = ShiftCategory.Create(user.Id, "Night", "#6366F1");
        await repo.AddAsync(cat);

        var found = await repo.GetByIdAsync(cat.Id);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Night");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        await using var db = CreateDb();
        var repo = new ShiftCategoryRepository(db);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task HasShiftsAsync_ReturnsTrue_WhenShiftsExist()
    {
        await using var db = CreateDb();
        var user = await SeedUserAsync(db);
        var repo = new ShiftCategoryRepository(db);
        var cat = ShiftCategory.Create(user.Id, "Cat", "#AABBCC");
        await repo.AddAsync(cat);

        var shift = Shift.Create(user.Id, cat.Id, DateOnly.FromDateTime(DateTime.Today),
            new TimeOnly(9, 0), new TimeOnly(17, 0), null, null);
        db.Shifts.Add(shift);
        await db.SaveChangesAsync();

        var hasShifts = await repo.HasShiftsAsync(cat.Id);

        hasShifts.Should().BeTrue();
    }

    [Fact]
    public async Task HasShiftsAsync_ReturnsFalse_WhenNoShifts()
    {
        await using var db = CreateDb();
        var user = await SeedUserAsync(db);
        var repo = new ShiftCategoryRepository(db);
        var cat = ShiftCategory.Create(user.Id, "Empty Cat", "#AABBCC");
        await repo.AddAsync(cat);

        var hasShifts = await repo.HasShiftsAsync(cat.Id);

        hasShifts.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        await using var db = CreateDb();
        var user = await SeedUserAsync(db);
        var repo = new ShiftCategoryRepository(db);
        var cat = ShiftCategory.Create(user.Id, "Old", "#000000");
        await repo.AddAsync(cat);

        cat.Update("New", "#FFFFFF");
        await repo.UpdateAsync(cat);

        var updated = await repo.GetByIdAsync(cat.Id);
        updated!.Name.Should().Be("New");
        updated.Color.Should().Be("#FFFFFF");
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategory()
    {
        await using var db = CreateDb();
        var user = await SeedUserAsync(db);
        var repo = new ShiftCategoryRepository(db);
        var cat = ShiftCategory.Create(user.Id, "ToDelete", "#AABBCC");
        await repo.AddAsync(cat);

        await repo.DeleteAsync(cat);

        var found = await repo.GetByIdAsync(cat.Id);
        found.Should().BeNull();
    }
}
