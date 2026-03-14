using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.Shifts;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.Shifts;

public class DeleteShiftCommandHandlerTests
{
    private readonly IShiftRepository _shiftRepo = Substitute.For<IShiftRepository>();
    private readonly DeleteShiftCommandHandler _handler;

    public DeleteShiftCommandHandlerTests()
    {
        _handler = new DeleteShiftCommandHandler(_shiftRepo);
    }

    [Fact]
    public async Task Handle_OwnShift_DeletesSuccessfully()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var shift = Shift.Create(userId, categoryId, new DateOnly(2026, 3, 1),
            new TimeOnly(7, 0), new TimeOnly(15, 0), null, null);

        _shiftRepo.GetByIdAsync(shift.Id, default).Returns(shift);
        _shiftRepo.DeleteAsync(shift, default).Returns(Task.CompletedTask);

        await _handler.Handle(new DeleteShiftCommand(shift.Id, userId), default);

        await _shiftRepo.Received(1).DeleteAsync(shift, default);
    }

    [Fact]
    public async Task Handle_ShiftNotFound_ThrowsKeyNotFoundException()
    {
        _shiftRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Shift?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(new DeleteShiftCommand(Guid.NewGuid(), Guid.NewGuid()), default));
    }

    [Fact]
    public async Task Handle_OtherUsersShift_ThrowsUnauthorized()
    {
        var shift = Shift.Create(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 3, 1),
            new TimeOnly(7, 0), new TimeOnly(15, 0), null, null);
        _shiftRepo.GetByIdAsync(shift.Id, default).Returns(shift);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new DeleteShiftCommand(shift.Id, Guid.NewGuid()), default));
    }
}
