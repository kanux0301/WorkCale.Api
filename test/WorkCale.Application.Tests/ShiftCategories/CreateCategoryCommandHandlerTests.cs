using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.ShiftCategories;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.ShiftCategories;

public class CreateCategoryCommandHandlerTests
{
    private readonly IShiftCategoryRepository _repo = Substitute.For<IShiftCategoryRepository>();
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _handler = new CreateCategoryCommandHandler(_repo);
        _repo.AddAsync(Arg.Any<ShiftCategory>(), default).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsDto()
    {
        var userId = Guid.NewGuid();
        var command = new CreateCategoryCommand(userId, "Evening Shift", "#EC4899");

        var result = await _handler.Handle(command, default);

        result.Name.Should().Be("Evening Shift");
        result.Color.Should().Be("#EC4899");
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithValidData_PersistsCategory()
    {
        var userId = Guid.NewGuid();
        var command = new CreateCategoryCommand(userId, "Overnight", "#8B5CF6");

        await _handler.Handle(command, default);

        await _repo.Received(1).AddAsync(
            Arg.Is<ShiftCategory>(c => c.Name == "Overnight" && c.UserId == userId),
            default);
    }
}
