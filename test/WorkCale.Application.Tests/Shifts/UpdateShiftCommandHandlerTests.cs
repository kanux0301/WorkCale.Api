using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.Shifts;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.Shifts;

public class UpdateShiftCommandHandlerTests
{
    private readonly IShiftRepository _shiftRepo = Substitute.For<IShiftRepository>();
    private readonly IShiftCategoryRepository _categoryRepo = Substitute.For<IShiftCategoryRepository>();
    private readonly UpdateShiftCommandHandler _handler;

    public UpdateShiftCommandHandlerTests()
    {
        _handler = new UpdateShiftCommandHandler(_shiftRepo, _categoryRepo);
        _shiftRepo.UpdateAsync(Arg.Any<Shift>(), default).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_WithValidData_UpdatesAndReturnsDto()
    {
        var userId = Guid.NewGuid();
        var category = ShiftCategory.Create(userId, "Night Shift", "#6366F1");
        var shift = Shift.Create(userId, category.Id, new DateOnly(2026, 3, 10),
            new TimeOnly(8, 0), new TimeOnly(16, 0), null, null);

        _shiftRepo.GetByIdAsync(shift.Id, default).Returns(shift);
        _categoryRepo.GetByIdAsync(category.Id, default).Returns(category);

        var result = await _handler.Handle(
            new UpdateShiftCommand(shift.Id, userId, new DateOnly(2026, 3, 11),
                "19:00", "07:00", category.Id, "ICU", "Busy night"), default);

        result.StartTime.Should().Be("19:00");
        result.Location.Should().Be("ICU");
        result.Notes.Should().Be("Busy night");
        await _shiftRepo.Received(1).UpdateAsync(shift, default);
    }

    [Fact]
    public async Task Handle_WhenShiftNotFound_ThrowsKeyNotFoundException()
    {
        _shiftRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Shift?)null);

        var act = () => _handler.Handle(
            new UpdateShiftCommand(Guid.NewGuid(), Guid.NewGuid(),
                new DateOnly(2026, 3, 1), "08:00", "16:00", Guid.NewGuid(), null, null), default);
        // Note: first Guid is ShiftId, second is UserId

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Shift not found*");
    }

    [Fact]
    public async Task Handle_WhenShiftOwnedByOtherUser_ThrowsUnauthorized()
    {
        var ownerId = Guid.NewGuid();
        var category = ShiftCategory.Create(ownerId, "Day", "#F59E0B");
        var shift = Shift.Create(ownerId, category.Id, new DateOnly(2026, 3, 10),
            new TimeOnly(8, 0), new TimeOnly(16, 0), null, null);

        _shiftRepo.GetByIdAsync(shift.Id, default).Returns(shift);

        var act = () => _handler.Handle(
            new UpdateShiftCommand(shift.Id, Guid.NewGuid(),
                new DateOnly(2026, 3, 10), "08:00", "16:00", category.Id, null, null), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*own this shift*");
    }

    [Fact]
    public async Task Handle_WhenCategoryOwnedByOtherUser_ThrowsUnauthorized()
    {
        var userId = Guid.NewGuid();
        var otherUser = Guid.NewGuid();
        var category = ShiftCategory.Create(otherUser, "Stolen Cat", "#F59E0B");
        var shift = Shift.Create(userId, category.Id, new DateOnly(2026, 3, 10),
            new TimeOnly(8, 0), new TimeOnly(16, 0), null, null);

        _shiftRepo.GetByIdAsync(shift.Id, default).Returns(shift);
        _categoryRepo.GetByIdAsync(category.Id, default).Returns(category);

        var act = () => _handler.Handle(
            new UpdateShiftCommand(shift.Id, userId,
                new DateOnly(2026, 3, 10), "08:00", "16:00", category.Id, null, null), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*own this category*");
    }
}
