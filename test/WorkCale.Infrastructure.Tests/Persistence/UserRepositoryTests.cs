using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WorkCale.Domain.Entities;
using WorkCale.Infrastructure.Persistence;
using WorkCale.Infrastructure.Persistence.Repositories;

namespace WorkCale.Infrastructure.Tests.Persistence;

public class UserRepositoryTests
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

    [Fact]
    public async Task AddAsync_PersistsUser()
    {
        await using var db = CreateDb();
        var repo = new UserRepository(db);
        var user = User.Create("a@b.com", "Alice", "hash");

        await repo.AddAsync(user);

        db.Users.Should().ContainSingle(u => u.Email == "a@b.com");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenExists()
    {
        await using var db = CreateDb();
        var repo = new UserRepository(db);
        var user = User.Create("a@b.com", "Alice", "hash");
        await repo.AddAsync(user);

        var found = await repo.GetByIdAsync(user.Id);

        found.Should().NotBeNull();
        found!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        await using var db = CreateDb();
        var repo = new UserRepository(db);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsUser_CaseInsensitive()
    {
        await using var db = CreateDb();
        var repo = new UserRepository(db);
        var user = User.Create("alice@example.com", "Alice", "hash");
        await repo.AddAsync(user);

        var found = await repo.GetByEmailAsync("ALICE@EXAMPLE.COM");

        found.Should().NotBeNull();
        found!.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsNull_WhenNotFound()
    {
        await using var db = CreateDb();
        var repo = new UserRepository(db);

        var result = await repo.GetByEmailAsync("nobody@x.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByGoogleIdAsync_ReturnsUser_WhenExists()
    {
        await using var db = CreateDb();
        var repo = new UserRepository(db);
        var user = User.CreateWithGoogle("g@x.com", "Google User", "gid999", null);
        await repo.AddAsync(user);

        var found = await repo.GetByGoogleIdAsync("gid999");

        found.Should().NotBeNull();
        found!.GoogleId.Should().Be("gid999");
    }

    [Fact]
    public async Task GetByGoogleIdAsync_ReturnsNull_WhenNotFound()
    {
        await using var db = CreateDb();
        var repo = new UserRepository(db);

        var result = await repo.GetByGoogleIdAsync("unknown");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_ReturnsMatchingByEmail()
    {
        await using var db = CreateDb();
        var repo = new UserRepository(db);
        await repo.AddAsync(User.Create("alice@work.com", "Alice", "h"));
        await repo.AddAsync(User.Create("bob@work.com", "Bob", "h"));

        var results = (await repo.SearchAsync("alice")).ToList();

        results.Should().ContainSingle(u => u.Email == "alice@work.com");
    }

    [Fact]
    public async Task SearchAsync_ReturnsMatchingByDisplayName()
    {
        await using var db = CreateDb();
        var repo = new UserRepository(db);
        await repo.AddAsync(User.Create("x@x.com", "Charlie Brown", "h"));
        await repo.AddAsync(User.Create("y@y.com", "David", "h"));

        var results = (await repo.SearchAsync("charlie")).ToList();

        results.Should().ContainSingle(u => u.DisplayName == "Charlie Brown");
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenNoMatch()
    {
        await using var db = CreateDb();
        var repo = new UserRepository(db);
        await repo.AddAsync(User.Create("x@x.com", "Alice", "h"));

        var results = await repo.SearchAsync("zzznomatch");

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        await using var db = CreateDb();
        var repo = new UserRepository(db);
        var user = User.Create("a@b.com", "Old Name", "hash");
        await repo.AddAsync(user);

        user.UpdateProfile("New Name");
        await repo.UpdateAsync(user);

        var updated = await repo.GetByIdAsync(user.Id);
        updated!.DisplayName.Should().Be("New Name");
    }
}
