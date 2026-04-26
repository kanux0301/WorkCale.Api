using System.Text.RegularExpressions;
using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.Invites;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.Invites;

public class InvitesHandlerTests
{
    private readonly IInviteCodeRepository _repo = Substitute.For<IInviteCodeRepository>();

    [Fact]
    public async Task CreateInvites_ProducesRequestedCount()
    {
        var sut = new CreateInvitesCommandHandler(_repo);

        var result = await sut.Handle(
            new CreateInvitesCommand(Guid.NewGuid(), Count: 3, ExpiresInHours: null, Note: null),
            CancellationToken.None);

        result.Created.Should().HaveCount(3);
        await _repo.Received(3).AddAsync(Arg.Any<InviteCode>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInvites_GeneratesCodesInExpectedFormat()
    {
        var sut = new CreateInvitesCommandHandler(_repo);

        var result = await sut.Handle(
            new CreateInvitesCommand(Guid.NewGuid(), Count: 5, ExpiresInHours: null, Note: null),
            CancellationToken.None);

        var pattern = new Regex("^WC-[A-Z2-9]{4}-[A-Z2-9]{4}$");
        foreach (var dto in result.Created)
            pattern.IsMatch(dto.Code).Should().BeTrue($"code '{dto.Code}' should match WC-XXXX-XXXX with unambiguous chars");
    }

    [Fact]
    public async Task CreateInvites_GeneratesUniqueCodes()
    {
        var sut = new CreateInvitesCommandHandler(_repo);

        var result = await sut.Handle(
            new CreateInvitesCommand(Guid.NewGuid(), Count: 50, ExpiresInHours: null, Note: null),
            CancellationToken.None);

        result.Created.Select(c => c.Code).Distinct().Should().HaveCount(50);
    }

    [Fact]
    public async Task CreateInvites_WithExpiresInHours_SetsAbsoluteExpiry()
    {
        InviteCode? captured = null;
        await _repo.AddAsync(Arg.Do<InviteCode>(i => captured = i), Arg.Any<CancellationToken>());
        var sut = new CreateInvitesCommandHandler(_repo);

        var before = DateTime.UtcNow;
        await sut.Handle(
            new CreateInvitesCommand(Guid.NewGuid(), Count: 1, ExpiresInHours: 24, Note: "n"),
            CancellationToken.None);

        captured!.ExpiresAt.Should().NotBeNull();
        captured.ExpiresAt!.Value.Should().BeCloseTo(before.AddHours(24), TimeSpan.FromSeconds(5));
        captured.Note.Should().Be("n");
    }

    [Fact]
    public async Task ListInvites_MapsAllToDto()
    {
        _repo.ListAsync(Arg.Any<CancellationToken>()).Returns(new List<InviteCode>
        {
            InviteCode.Issue(Guid.NewGuid(), "WC-AAAA-BBBB", null, null),
            InviteCode.Issue(Guid.NewGuid(), "WC-CCCC-DDDD", null, "note"),
        });
        var sut = new ListInvitesQueryHandler(_repo);

        var result = await sut.Handle(new ListInvitesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(r => r.Code).Should().Contain(["WC-AAAA-BBBB", "WC-CCCC-DDDD"]);
    }
}
