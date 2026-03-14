using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.Shifts;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.Shifts;

public class GetShiftsQueryHandlerTests
{
    private readonly IShiftRepository _repo = Substitute.For<IShiftRepository>();
    private readonly GetShiftsQueryHandler _handler;

    public GetShiftsQueryHandlerTests()
    {
        _handler = new GetShiftsQueryHandler(_repo);
    }

    private static Shift MakeShift(Guid userId, ShiftCategory category)
    {
        var shift = Shift.Create(userId, category.Id, new DateOnly(2026, 3, 10),
            new TimeOnly(8, 0), new TimeOnly(16, 0), null, null);
        // Attach navigation property via reflection for test purposes
        typeof(Shift).GetProperty("Category")!
            .SetValue(shift, category);
        return shift;
    }

    [Fact]
    public async Task Handle_ReturnsMappedDtosForMonth()
    {
        var userId = Guid.NewGuid();
        var category = ShiftCategory.Create(userId, "Day Shift", "#F59E0B");
        var shift = MakeShift(userId, category);
        _repo.GetByUserAndMonthAsync(userId, 2026, 3, default).Returns([shift]);

        var result = (await _handler.Handle(new GetShiftsQuery(userId, 2026, 3), default)).ToList();

        result.Should().HaveCount(1);
        result[0].StartTime.Should().Be("08:00");
        result[0].Category.Name.Should().Be("Day Shift");
    }

    [Fact]
    public async Task Handle_WhenNoShifts_ReturnsEmpty()
    {
        var userId = Guid.NewGuid();
        _repo.GetByUserAndMonthAsync(userId, 2026, 3, default).Returns([]);

        var result = await _handler.Handle(new GetShiftsQuery(userId, 2026, 3), default);

        result.Should().BeEmpty();
    }
}
