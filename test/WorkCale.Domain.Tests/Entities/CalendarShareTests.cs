using FluentAssertions;
using WorkCale.Domain.Entities;

namespace WorkCale.Domain.Tests.Entities;

public class CalendarShareTests
{
    [Fact]
    public void Create_SetsPropertiesCorrectly()
    {
        var ownerId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();

        var share = CalendarShare.Create(ownerId, viewerId);

        share.Id.Should().NotBeEmpty();
        share.OwnerUserId.Should().Be(ownerId);
        share.ViewerUserId.Should().Be(viewerId);
        share.IsActive.Should().BeTrue();
        share.RevokedAt.Should().BeNull();
        share.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Revoke_SetsIsActiveFalse()
    {
        var share = CalendarShare.Create(Guid.NewGuid(), Guid.NewGuid());

        share.Revoke();

        share.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Revoke_SetsRevokedAt()
    {
        var share = CalendarShare.Create(Guid.NewGuid(), Guid.NewGuid());

        share.Revoke();

        share.RevokedAt.Should().NotBeNull();
        share.RevokedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_TwoSharesHaveDifferentIds()
    {
        var share1 = CalendarShare.Create(Guid.NewGuid(), Guid.NewGuid());
        var share2 = CalendarShare.Create(Guid.NewGuid(), Guid.NewGuid());

        share1.Id.Should().NotBe(share2.Id);
    }
}
