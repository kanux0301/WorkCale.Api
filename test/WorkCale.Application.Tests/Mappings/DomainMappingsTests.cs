using FluentAssertions;
using WorkCale.Application.Mappings;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.Mappings;

public class DomainMappingsTests
{
    [Fact]
    public void User_ToDto_CopiesAllFields()
    {
        var user = User.Create("a@b.com", "Alice", "hash");
        user.UpdateProfile("Alice Updated", "#4C6FA3", "person");

        var dto = user.ToDto();

        dto.Id.Should().Be(user.Id);
        dto.Email.Should().Be("a@b.com");
        dto.DisplayName.Should().Be("Alice Updated");
        dto.AvatarColor.Should().Be("#4C6FA3");
        dto.AvatarIcon.Should().Be("person");
    }

    [Fact]
    public void ShiftCategory_ToDto_CopiesAllFields()
    {
        var category = ShiftCategory.Create(Guid.NewGuid(), Guid.NewGuid(), "Day", "#F59E0B", "08:00", "16:00", "sun");

        var dto = category.ToDto();

        dto.Id.Should().Be(category.Id);
        dto.Name.Should().Be("Day");
        dto.Color.Should().Be("#F59E0B");
        dto.DefaultStartTime.Should().Be("08:00");
        dto.DefaultEndTime.Should().Be("16:00");
        dto.Icon.Should().Be("sun");
    }

    [Fact]
    public void Shift_ToDto_WithExplicitCategory_FormatsTimesAsHHmm()
    {
        var category = ShiftCategory.Create(Guid.NewGuid(), Guid.NewGuid(), "Day", "#F59E0B", null, null);
        var shift = Shift.Create(Guid.NewGuid(), Guid.NewGuid(), category.Id, new DateOnly(2026, 4, 25), new TimeOnly(9, 5), new TimeOnly(17, 0), "Office", "Note", 30);

        var dto = shift.ToDto(category);

        dto.StartTime.Should().Be("09:05");
        dto.EndTime.Should().Be("17:00");
        dto.UnpaidBreakMinutes.Should().Be(30);
        dto.Location.Should().Be("Office");
        dto.Category.Id.Should().Be(category.Id);
    }

    [Fact]
    public void InviteCode_ToDto_CopiesAllFields()
    {
        var invite = InviteCode.Issue(
            Guid.NewGuid(), "WC-AB12-CD34",
            DateTime.UtcNow.AddDays(7), "test note");

        var dto = invite.ToDto();

        dto.Id.Should().Be(invite.Id);
        dto.Code.Should().Be("WC-AB12-CD34");
        dto.Note.Should().Be("test note");
        dto.ConsumedAt.Should().BeNull();
        dto.ConsumedByUserId.Should().BeNull();
    }
}
