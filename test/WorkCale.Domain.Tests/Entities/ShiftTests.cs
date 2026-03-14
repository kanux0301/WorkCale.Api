using FluentAssertions;
using WorkCale.Domain.Entities;

namespace WorkCale.Domain.Tests.Entities;

public class ShiftTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly DateOnly Date = new(2026, 3, 15);
    private static readonly TimeOnly Start = new(9, 0);
    private static readonly TimeOnly End = new(17, 0);

    [Fact]
    public void Create_SetsAllProperties()
    {
        var shift = Shift.Create(UserId, CategoryId, Date, Start, End, "Office", "Notes here");

        shift.Id.Should().NotBeEmpty();
        shift.UserId.Should().Be(UserId);
        shift.CategoryId.Should().Be(CategoryId);
        shift.Date.Should().Be(Date);
        shift.StartTime.Should().Be(Start);
        shift.EndTime.Should().Be(End);
        shift.Location.Should().Be("Office");
        shift.Notes.Should().Be("Notes here");
        shift.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        shift.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_AllowsNullLocationAndNotes()
    {
        var shift = Shift.Create(UserId, CategoryId, Date, Start, End, null, null);
        shift.Location.Should().BeNull();
        shift.Notes.Should().BeNull();
    }

    [Fact]
    public void Update_ChangesFields()
    {
        var shift = Shift.Create(UserId, CategoryId, Date, Start, End, "Office", "Old notes");
        var newCatId = Guid.NewGuid();
        var newDate = new DateOnly(2026, 4, 1);
        var beforeUpdate = shift.UpdatedAt;

        shift.Update(newCatId, newDate, new TimeOnly(8, 0), new TimeOnly(16, 0), "Home", "New notes");

        shift.CategoryId.Should().Be(newCatId);
        shift.Date.Should().Be(newDate);
        shift.StartTime.Should().Be(new TimeOnly(8, 0));
        shift.EndTime.Should().Be(new TimeOnly(16, 0));
        shift.Location.Should().Be("Home");
        shift.Notes.Should().Be("New notes");
        shift.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void Update_DoesNotChangeCreatedAt()
    {
        var shift = Shift.Create(UserId, CategoryId, Date, Start, End, null, null);
        var created = shift.CreatedAt;

        shift.Update(CategoryId, Date, Start, End, null, null);

        shift.CreatedAt.Should().Be(created);
    }
}
