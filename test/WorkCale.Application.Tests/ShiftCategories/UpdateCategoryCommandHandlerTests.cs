using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.ShiftCategories;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.ShiftCategories;

public class UpdateCategoryCommandHandlerTests
{
    private readonly IShiftCategoryRepository _repo = Substitute.For<IShiftCategoryRepository>();
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        _handler = new UpdateCategoryCommandHandler(_repo);
        _repo.UpdateAsync(Arg.Any<ShiftCategory>(), default).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_WithValidOwner_UpdatesAndReturnsDto()
    {
        var userId = Guid.NewGuid();
        var category = ShiftCategory.Create(userId, "Day Shift", "#F59E0B");
        _repo.GetByIdAsync(category.Id, default).Returns(category);

        var result = await _handler.Handle(
            new UpdateCategoryCommand(category.Id, userId, "Morning Shift", "#10B981"), default);

        result.Name.Should().Be("Morning Shift");
        result.Color.Should().Be("#10B981");
        await _repo.Received(1).UpdateAsync(category, default);
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ThrowsKeyNotFoundException()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((ShiftCategory?)null);

        var act = () => _handler.Handle(
            new UpdateCategoryCommand(Guid.NewGuid(), Guid.NewGuid(), "Name", "#F59E0B"), default);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_WhenCategoryOwnedByOtherUser_ThrowsUnauthorized()
    {
        var ownerUserId = Guid.NewGuid();
        var category = ShiftCategory.Create(ownerUserId, "Day", "#F59E0B");
        _repo.GetByIdAsync(category.Id, default).Returns(category);

        var act = () => _handler.Handle(
            new UpdateCategoryCommand(category.Id, Guid.NewGuid(), "Changed", "#F59E0B"), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
