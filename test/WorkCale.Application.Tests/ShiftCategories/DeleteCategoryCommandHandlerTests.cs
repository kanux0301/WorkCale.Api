using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.ShiftCategories;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.ShiftCategories;

public class DeleteCategoryCommandHandlerTests
{
    private readonly IShiftCategoryRepository _repo = Substitute.For<IShiftCategoryRepository>();
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        _handler = new DeleteCategoryCommandHandler(_repo);
    }

    [Fact]
    public async Task Handle_CategoryWithNoShifts_DeletesSuccessfully()
    {
        var userId = Guid.NewGuid();
        var category = ShiftCategory.Create(userId, "Day", "#F59E0B");

        _repo.GetByIdAsync(category.Id, default).Returns(category);
        _repo.HasShiftsAsync(category.Id, default).Returns(false);
        _repo.DeleteAsync(category, default).Returns(Task.CompletedTask);

        await _handler.Handle(new DeleteCategoryCommand(category.Id, userId), default);

        await _repo.Received(1).DeleteAsync(category, default);
    }

    [Fact]
    public async Task Handle_CategoryWithShifts_ThrowsInvalidOperation()
    {
        var userId = Guid.NewGuid();
        var category = ShiftCategory.Create(userId, "Day", "#F59E0B");

        _repo.GetByIdAsync(category.Id, default).Returns(category);
        _repo.HasShiftsAsync(category.Id, default).Returns(true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new DeleteCategoryCommand(category.Id, userId), default));
    }

    [Fact]
    public async Task Handle_OtherUsersCategory_ThrowsUnauthorized()
    {
        var category = ShiftCategory.Create(Guid.NewGuid(), "Day", "#F59E0B");
        _repo.GetByIdAsync(category.Id, default).Returns(category);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new DeleteCategoryCommand(category.Id, Guid.NewGuid()), default));
    }
}
