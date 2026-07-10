using WorkCale.Application.DTOs;
using WorkCale.Domain.Entities;

namespace WorkCale.Application.Mappings;

public static class DomainMappings
{
    private const string TimeFormat = "HH:mm";

    public static UserDto ToDto(this User user) =>
        new(user.Id, user.Email, user.DisplayName, user.AvatarUrl, user.AvatarColor, user.AvatarIcon);

    public static JobDto ToDto(this Job job) =>
        new(job.Id, job.Name, job.Color, job.Icon, job.IsDefault, job.IsArchived, job.SortOrder, job.CreatedAt);

    public static ShiftCategoryDto ToDto(this ShiftCategory category) =>
        new(category.Id, category.JobId, category.Name, category.Color,
            category.DefaultStartTime, category.DefaultEndTime, category.Icon, category.CreatedAt);

    public static ShiftDto ToDto(this Shift shift) =>
        new(shift.Id, shift.JobId, shift.Date,
            shift.StartTime.ToString(TimeFormat),
            shift.EndTime.ToString(TimeFormat),
            shift.Location, shift.Notes, shift.UnpaidBreakMinutes,
            shift.CreatedAt, shift.UpdatedAt,
            shift.Category.ToDto());

    public static ShiftDto ToDto(this Shift shift, ShiftCategory category) =>
        new(shift.Id, shift.JobId, shift.Date,
            shift.StartTime.ToString(TimeFormat),
            shift.EndTime.ToString(TimeFormat),
            shift.Location, shift.Notes, shift.UnpaidBreakMinutes,
            shift.CreatedAt, shift.UpdatedAt,
            category.ToDto());

    public static InviteCodeDto ToDto(this InviteCode invite) =>
        new(invite.Id, invite.Code, invite.IssuedAt, invite.ExpiresAt,
            invite.ConsumedAt, invite.ConsumedByUserId, invite.Note);
}
