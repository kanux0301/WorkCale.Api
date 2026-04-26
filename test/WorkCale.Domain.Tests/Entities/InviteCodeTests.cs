using FluentAssertions;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Domain.Tests.Entities;

public class InviteCodeTests
{
    [Fact]
    public void Issue_PopulatesIssuerAndCode_LeavesUnconsumed()
    {
        var issuer = Guid.NewGuid();

        var invite = InviteCode.Issue(issuer, "WC-AB12-CD34", null, "for Bob");

        invite.IssuedByUserId.Should().Be(issuer);
        invite.Code.Should().Be("WC-AB12-CD34");
        invite.Note.Should().Be("for Bob");
        invite.ConsumedByUserId.Should().BeNull();
        invite.ConsumedAt.Should().BeNull();
        invite.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void IsRedeemable_NewInvite_True()
    {
        var invite = InviteCode.Issue(Guid.NewGuid(), "code", null, null);

        invite.IsRedeemable(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsRedeemable_Expired_False()
    {
        var now = DateTime.UtcNow;
        var invite = InviteCode.Issue(Guid.NewGuid(), "code", now.AddHours(-1), null);

        invite.IsRedeemable(now).Should().BeFalse();
    }

    [Fact]
    public void IsRedeemable_AlreadyConsumed_False()
    {
        var invite = InviteCode.Issue(Guid.NewGuid(), "code", null, null);
        invite.Consume(Guid.NewGuid(), DateTime.UtcNow);

        invite.IsRedeemable(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void Consume_TwiceThrows()
    {
        var invite = InviteCode.Issue(Guid.NewGuid(), "code", null, null);
        invite.Consume(Guid.NewGuid(), DateTime.UtcNow);

        Action act = () => invite.Consume(Guid.NewGuid(), DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>().WithMessage("Invite code already used.");
    }

    [Fact]
    public void Consume_RecordsUserAndTimestamp()
    {
        var invite = InviteCode.Issue(Guid.NewGuid(), "code", null, null);
        var consumer = Guid.NewGuid();
        var when = new DateTime(2026, 4, 25, 12, 0, 0, DateTimeKind.Utc);

        invite.Consume(consumer, when);

        invite.ConsumedByUserId.Should().Be(consumer);
        invite.ConsumedAt.Should().Be(when);
    }
}
