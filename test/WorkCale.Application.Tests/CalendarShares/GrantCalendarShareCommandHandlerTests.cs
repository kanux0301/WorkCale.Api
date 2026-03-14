using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.CalendarShares;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.CalendarShares;

public class GrantCalendarShareCommandHandlerTests
{
    private readonly ICalendarShareRepository _shareRepo = Substitute.For<ICalendarShareRepository>();
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly GrantCalendarShareCommandHandler _handler;

    public GrantCalendarShareCommandHandlerTests()
    {
        _handler = new GrantCalendarShareCommandHandler(_shareRepo, _userRepo);
    }

    [Fact]
    public async Task Handle_NewShare_CreatesSuccessfully()
    {
        var ownerId = Guid.NewGuid();
        var viewerUser = User.Create("viewer@test.com", "Viewer", "hash");

        _userRepo.GetByIdAsync(viewerUser.Id, default).Returns(viewerUser);
        _shareRepo.GetActiveShareAsync(ownerId, viewerUser.Id, default).Returns((CalendarShare?)null);
        _shareRepo.AddAsync(Arg.Any<CalendarShare>(), default).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new GrantCalendarShareCommand(ownerId, viewerUser.Id), default);

        result.User.Id.Should().Be(viewerUser.Id);
        await _shareRepo.Received(1).AddAsync(Arg.Any<CalendarShare>(), default);
    }

    [Fact]
    public async Task Handle_DuplicateShare_ThrowsInvalidOperation()
    {
        var ownerId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var viewer = User.Create("v@test.com", "Viewer", "hash");

        _userRepo.GetByIdAsync(viewerId, default).Returns(viewer);
        _shareRepo.GetActiveShareAsync(ownerId, viewerId, default)
            .Returns(CalendarShare.Create(ownerId, viewerId));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new GrantCalendarShareCommand(ownerId, viewerId), default));
    }

    [Fact]
    public async Task Handle_ViewerNotFound_ThrowsKeyNotFoundException()
    {
        _userRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(new GrantCalendarShareCommand(Guid.NewGuid(), Guid.NewGuid()), default));
    }
}
