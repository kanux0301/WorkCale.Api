using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.Shifts;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.Shifts;

public class CreateShiftCommandHandlerTests
{
    private readonly IShiftRepository _shiftRepo = Substitute.For<IShiftRepository>();
    private readonly IShiftCategoryRepository _categoryRepo = Substitute.For<IShiftCategoryRepository>();
    private readonly CreateShiftCommandHandler _handler;

    public CreateShiftCommandHandlerTests()
    {
        _handler = new CreateShiftCommandHandler(_shiftRepo, _categoryRepo);
    }

    [Fact]
    public async Task Handle_WithValidCategory_CreatesShift()
    {
        var userId = Guid.NewGuid();
        var category = ShiftCategory.Create(userId, "Day Shift", "#F59E0B");

        _categoryRepo.GetByIdAsync(category.Id, default).Returns(category);
        _shiftRepo.AddAsync(Arg.Any<Shift>(), default).Returns(Task.CompletedTask);

        var command = new CreateShiftCommand(userId, new DateOnly(2026, 3, 15),
            "07:00", "15:00", category.Id, "Ward A", null);

        var result = await _handler.Handle(command, default);

        result.Should().NotBeNull();
        result.StartTime.Should().Be("07:00");
        result.Category.Id.Should().Be(category.Id);
        await _shiftRepo.Received(1).AddAsync(Arg.Any<Shift>(), default);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsKeyNotFoundException()
    {
        _categoryRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((ShiftCategory?)null);

        var command = new CreateShiftCommand(Guid.NewGuid(), new DateOnly(2026, 3, 15),
            "07:00", "15:00", Guid.NewGuid(), null, null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, default));
    }

    [Fact]
    public async Task Handle_CategoryOwnedByOtherUser_ThrowsUnauthorized()
    {
        var category = ShiftCategory.Create(Guid.NewGuid(), "Night", "#6366F1");
        _categoryRepo.GetByIdAsync(category.Id, default).Returns(category);

        var command = new CreateShiftCommand(Guid.NewGuid(), new DateOnly(2026, 3, 15),
            "19:00", "07:00", category.Id, null, null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command, default));
    }
}
