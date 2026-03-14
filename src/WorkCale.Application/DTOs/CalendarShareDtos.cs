using System.ComponentModel.DataAnnotations;

namespace WorkCale.Application.DTOs;

public record CalendarShareDto(
    Guid Id,
    UserDto User,
    DateTime CreatedAt);

public record MySharesDto(
    IEnumerable<CalendarShareDto> GrantedByMe,
    IEnumerable<CalendarShareDto> GrantedToMe);

public record GrantShareRequest(
    [Required] Guid ViewerUserId);

public record SharedCalendarDto(
    UserDto Owner,
    IEnumerable<ShiftDto> Shifts);
