using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
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

        return new SharedCalendarDto(owner.ToDto(), shifts.Select(s => s.ToDto()));
    }
}
