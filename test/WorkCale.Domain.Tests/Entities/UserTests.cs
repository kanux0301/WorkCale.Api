using FluentAssertions;
using WorkCale.Domain.Entities;

namespace WorkCale.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_SetsPropertiesCorrectly()
    {
        var user = User.Create("Test@Example.COM", "Alice", "hash123");

        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be("test@example.com");
        user.DisplayName.Should().Be("Alice");
        user.PasswordHash.Should().Be("hash123");
        user.GoogleId.Should().BeNull();
        user.AvatarUrl.Should().BeNull();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_LowercasesEmail()
    {
        var user = User.Create("UPPER@CASE.COM", "Bob", "h");
        user.Email.Should().Be("upper@case.com");
    }

    [Fact]
    public void CreateWithGoogle_SetsPropertiesCorrectly()
    {
        var user = User.CreateWithGoogle("G@EXAMPLE.COM", "Google User", "gid123", "https://avatar.url");

        user.Email.Should().Be("g@example.com");
        user.DisplayName.Should().Be("Google User");
        user.GoogleId.Should().Be("gid123");
        user.AvatarUrl.Should().Be("https://avatar.url");
        user.PasswordHash.Should().BeNull();
    }

    [Fact]
    public void CreateWithGoogle_NullAvatarIsAllowed()
    {
        var user = User.CreateWithGoogle("x@x.com", "X", "gid", null);
        user.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public void LinkGoogle_UpdatesGoogleIdAndAvatar()
    {
        var user = User.Create("a@b.com", "Alice", "hash");
        var before = user.UpdatedAt;

        user.LinkGoogle("newgid", "https://new.avatar");

        user.GoogleId.Should().Be("newgid");
        user.AvatarUrl.Should().Be("https://new.avatar");
        user.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void UpdateProfile_ChangesDisplayName()
    {
        var user = User.Create("a@b.com", "Old Name", "hash");
        user.UpdateProfile("New Name");
        user.DisplayName.Should().Be("New Name");
    }

    [Fact]
    public void UpdatePassword_ChangesPasswordHash()
    {
        var user = User.Create("a@b.com", "Alice", "oldhash");
        user.UpdatePassword("newhash");
        user.PasswordHash.Should().Be("newhash");
    }
}
