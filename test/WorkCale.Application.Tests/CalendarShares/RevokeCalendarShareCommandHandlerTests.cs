using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.CalendarShares;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.CalendarShares;

public class RevokeCalendarShareCommandHandlerTests
{
    private readonly ICalendarShareRepository _repo = Substitute.For<ICalendarShareRepository>();
    private readonly RevokeCalendarShareCommandHandler _handler;

    public RevokeCalendarShareCommandHandlerTests()
    {
        _handler = new RevokeCalendarShareCommandHandler(_repo);
        _repo.UpdateAsync(Arg.Any<CalendarShare>(), default).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_WithValidOwner_RevokesShare()
    {
        var ownerId = Guid.NewGuid();
        var share = CalendarShare.Create(ownerId, Guid.NewGuid());
        _repo.GetByIdAsync(share.Id, default).Returns(share);

        await _handler.Handle(new RevokeCalendarShareCommand(share.Id, ownerId), default);

        share.IsActive.Should().BeFalse();
        share.RevokedAt.Should().NotBeNull();
        await _repo.Received(1).UpdateAsync(share, default);
    }

    [Fact]
    public async Task Handle_WhenShareNotFound_ThrowsKeyNotFoundException()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((CalendarShare?)null);

        var act = () => _handler.Handle(
            new RevokeCalendarShareCommand(Guid.NewGuid(), Guid.NewGuid()), default);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_WhenCalledByNonOwner_ThrowsUnauthorized()
    {
        var ownerId = Guid.NewGuid();
        var share = CalendarShare.Create(ownerId, Guid.NewGuid());
        _repo.GetByIdAsync(share.Id, default).Returns(share);

        var act = () => _handler.Handle(
            new RevokeCalendarShareCommand(share.Id, Guid.NewGuid()), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
