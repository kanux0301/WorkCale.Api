using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.ShiftCategories;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.ShiftCategories;

public class GetCategoriesQueryHandlerTests
{
    private readonly IShiftCategoryRepository _repo = Substitute.For<IShiftCategoryRepository>();
    private readonly GetCategoriesQueryHandler _handler;

    public GetCategoriesQueryHandlerTests()
    {
        _handler = new GetCategoriesQueryHandler(_repo);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyUserCategories()
    {
        var userId = Guid.NewGuid();
        var categories = new List<ShiftCategory>
        {
            ShiftCategory.Create(userId, "Day Shift", "#F59E0B"),
            ShiftCategory.Create(userId, "Night Shift", "#6366F1"),
        };
        _repo.GetByUserIdAsync(userId, default).Returns(categories);

        var result = (await _handler.Handle(new GetCategoriesQuery(userId), default)).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Day Shift" && c.Color == "#F59E0B");
        result.Should().Contain(c => c.Name == "Night Shift");
    }

    [Fact]
    public async Task Handle_WhenNoCategories_ReturnsEmpty()
    {
        var userId = Guid.NewGuid();
        _repo.GetByUserIdAsync(userId, default).Returns([]);

        var result = await _handler.Handle(new GetCategoriesQuery(userId), default);

        result.Should().BeEmpty();
    }
}
