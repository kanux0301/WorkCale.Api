using FluentAssertions;
using WorkCale.Domain.Entities;

namespace WorkCale.Domain.Tests.Entities;

public class ShiftCategoryTests
{
    [Fact]
    public void Create_SetsPropertiesCorrectly()
    {
        var userId = Guid.NewGuid();
        var cat = ShiftCategory.Create(userId, "Day Shift", "#F59E0B");

        cat.Id.Should().NotBeEmpty();
        cat.UserId.Should().Be(userId);
        cat.Name.Should().Be("Day Shift");
        cat.Color.Should().Be("#F59E0B");
        cat.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        cat.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        cat.Shifts.Should().BeEmpty();
    }

    [Fact]
    public void Update_ChangesNameAndColor()
    {
        var cat = ShiftCategory.Create(Guid.NewGuid(), "Old", "#000000");
        var beforeUpdate = cat.UpdatedAt;

        cat.Update("New Name", "#FFFFFF");

        cat.Name.Should().Be("New Name");
        cat.Color.Should().Be("#FFFFFF");
        cat.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void Update_DoesNotChangeCreatedAt()
    {
        var cat = ShiftCategory.Create(Guid.NewGuid(), "Cat", "#AABBCC");
        var created = cat.CreatedAt;

        cat.Update("New", "#112233");

        cat.CreatedAt.Should().Be(created);
    }
}
