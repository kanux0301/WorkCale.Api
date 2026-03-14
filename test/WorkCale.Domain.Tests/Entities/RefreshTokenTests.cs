using FluentAssertions;
using WorkCale.Domain.Entities;

namespace WorkCale.Domain.Tests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Create_SetsPropertiesCorrectly()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, "tok123");

        token.Id.Should().NotBeEmpty();
        token.UserId.Should().Be(userId);
        token.Token.Should().Be("tok123");
        token.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(10));
        token.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithCustomExpiry_UsesProvidedDays()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "tok", expiryDays: 7);
        token.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void IsExpired_ReturnsFalse_WhenNotExpired()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "tok", expiryDays: 30);
        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ReturnsTrue_WhenExpired()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "tok", expiryDays: 30);
        typeof(RefreshToken).GetProperty("ExpiresAt")!
            .SetValue(token, DateTime.UtcNow.AddDays(-1));

        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void Create_TwoTokensHaveDifferentIds()
    {
        var t1 = RefreshToken.Create(Guid.NewGuid(), "a");
        var t2 = RefreshToken.Create(Guid.NewGuid(), "b");
        t1.Id.Should().NotBe(t2.Id);
    }
}
