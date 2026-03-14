using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WorkCale.Domain.Entities;
using WorkCale.Infrastructure.Persistence;
using WorkCale.Infrastructure.Persistence.Repositories;

namespace WorkCale.Infrastructure.Tests.Persistence;

public class RefreshTokenRepositoryTests
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
    public async Task AddAsync_PersistsToken()
    {
        await using var db = CreateDb();
        var user = await SeedUserAsync(db);
        var repo = new RefreshTokenRepository(db);
        var token = RefreshToken.Create(user.Id, "tok123");

        await repo.AddAsync(token);

        db.RefreshTokens.Should().ContainSingle(t => t.Token == "tok123");
    }

    [Fact]
    public async Task GetByTokenAsync_ReturnsToken_WhenExists()
    {
        await using var db = CreateDb();
        var user = await SeedUserAsync(db);
        var repo = new RefreshTokenRepository(db);
        var token = RefreshToken.Create(user.Id, "abc123");
        await repo.AddAsync(token);

        var found = await repo.GetByTokenAsync("abc123");

        found.Should().NotBeNull();
        found!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByTokenAsync_ReturnsNull_WhenNotFound()
    {
        await using var db = CreateDb();
        var repo = new RefreshTokenRepository(db);

        var result = await repo.GetByTokenAsync("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_RemovesToken()
    {
        await using var db = CreateDb();
        var user = await SeedUserAsync(db);
        var repo = new RefreshTokenRepository(db);
        var token = RefreshToken.Create(user.Id, "tok_to_delete");
        await repo.AddAsync(token);

        await repo.DeleteAsync(token);

        (await repo.GetByTokenAsync("tok_to_delete")).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAllForUserAsync_RemovesAllUserTokens()
    {
        await using var db = CreateDb();
        var user = await SeedUserAsync(db);
        var otherUser = User.Create("other@test.com", "Other", "hash");
        db.Users.Add(otherUser);
        await db.SaveChangesAsync();

        var repo = new RefreshTokenRepository(db);
        await repo.AddAsync(RefreshToken.Create(user.Id, "tok1"));
        await repo.AddAsync(RefreshToken.Create(user.Id, "tok2"));
        await repo.AddAsync(RefreshToken.Create(otherUser.Id, "tok3"));

        await repo.DeleteAllForUserAsync(user.Id);

        db.RefreshTokens.Should().ContainSingle(t => t.UserId == otherUser.Id);
    }

    [Fact]
    public async Task DeleteAllForUserAsync_DoesNothing_WhenNoTokens()
    {
        await using var db = CreateDb();
        var user = await SeedUserAsync(db);
        var repo = new RefreshTokenRepository(db);

        var act = async () => await repo.DeleteAllForUserAsync(user.Id);

        await act.Should().NotThrowAsync();
    }
}
