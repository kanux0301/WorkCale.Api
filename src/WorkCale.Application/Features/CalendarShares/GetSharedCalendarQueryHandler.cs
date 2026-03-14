using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using MediatR;

namespace WorkCale.Application.Features.CalendarShares;

public class GetSharedCalendarQueryHandler(
    ICalendarShareRepository shareRepository,
    IUserRepository userRepository,
    IShiftRepository shiftRepository)
    : IRequestHandler<GetSharedCalendarQuery, SharedCalendarDto>
{
    public async Task<SharedCalendarDto> Handle(GetSharedCalendarQuery request, CancellationToken ct)
    {
        var share = await shareRepository.GetActiveShareAsync(request.OwnerUserId, request.ViewerUserId, ct);
        if (share is null)
            throw new UnauthorizedAccessException("You do not have access to this calendar.");

        var owner = await userRepository.GetByIdAsync(request.OwnerUserId, ct)
                    ?? throw new KeyNotFoundException("Owner not found.");

        var shifts = await shiftRepository.GetByUserAndMonthAsync(request.OwnerUserId, request.Year, request.Month, ct);

        var ownerDto = new UserDto(owner.Id, owner.Email, owner.DisplayName, owner.AvatarUrl);
        var shiftDtos = shifts.Select(s => new ShiftDto(
            s.Id, s.Date,
            s.StartTime.ToString("HH:mm"), s.EndTime.ToString("HH:mm"),
            s.Location, s.Notes, s.CreatedAt, s.UpdatedAt,
            new ShiftCategoryDto(s.Category.Id, s.Category.Name, s.Category.Color, s.Category.DefaultStartTime, s.Category.DefaultEndTime, s.Category.CreatedAt)));

        return new SharedCalendarDto(ownerDto, shiftDtos);
    }
}
