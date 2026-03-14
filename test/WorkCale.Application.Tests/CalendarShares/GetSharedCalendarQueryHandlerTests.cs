using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.CalendarShares;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.CalendarShares;

public class GetSharedCalendarQueryHandlerTests
{
    private readonly ICalendarShareRepository _shareRepo = Substitute.For<ICalendarShareRepository>();
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IShiftRepository _shiftRepo = Substitute.For<IShiftRepository>();
    private readonly GetSharedCalendarQueryHandler _handler;

    public GetSharedCalendarQueryHandlerTests()
    {
        _handler = new GetSharedCalendarQueryHandler(_shareRepo, _userRepo, _shiftRepo);
    }

    private static Shift MakeShift(Guid userId, ShiftCategory category)
    {
        var shift = Shift.Create(userId, category.Id, new DateOnly(2026, 3, 5),
            new TimeOnly(8, 0), new TimeOnly(16, 0), null, null);
        typeof(Shift).GetProperty("Category")!.SetValue(shift, category);
        return shift;
    }

    [Fact]
    public async Task Handle_WithActiveShare_ReturnsOwnerAndShifts()
    {
        var ownerId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var owner = User.Create("owner@test.com", "Owner", "hash");
        var category = ShiftCategory.Create(ownerId, "Day Shift", "#F59E0B");
        var share = CalendarShare.Create(ownerId, viewerId);
        var shift = MakeShift(ownerId, category);

        _shareRepo.GetActiveShareAsync(ownerId, viewerId, default).Returns(share);
        _userRepo.GetByIdAsync(ownerId, default).Returns(owner);
        _shiftRepo.GetByUserAndMonthAsync(ownerId, 2026, 3, default).Returns([shift]);

        var result = await _handler.Handle(
            new GetSharedCalendarQuery(viewerId, ownerId, 2026, 3), default);

        result.Owner.Email.Should().Be("owner@test.com");
        result.Shifts.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithNoActiveShare_ThrowsUnauthorized()
    {
        _shareRepo.GetActiveShareAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), default)
            .Returns((CalendarShare?)null);

        var act = () => _handler.Handle(
            new GetSharedCalendarQuery(Guid.NewGuid(), Guid.NewGuid(), 2026, 3), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*do not have access*");
    }

    [Fact]
    public async Task Handle_WhenOwnerNotFound_ThrowsKeyNotFoundException()
    {
        var ownerId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var share = CalendarShare.Create(ownerId, viewerId);
        _shareRepo.GetActiveShareAsync(ownerId, viewerId, default).Returns(share);
        _userRepo.GetByIdAsync(ownerId, default).Returns((User?)null);

        var act = () => _handler.Handle(
            new GetSharedCalendarQuery(viewerId, ownerId, 2026, 3), default);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Owner not found*");
    }
}
