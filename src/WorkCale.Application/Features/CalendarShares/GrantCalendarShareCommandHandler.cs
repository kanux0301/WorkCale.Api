using MediatR;
using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Application.Features.CalendarShares;

public class GrantCalendarShareCommandHandler(
    ICalendarShareRepository shareRepository,
    IUserRepository userRepository)
    : IRequestHandler<GrantCalendarShareCommand, CalendarShareDto>
{
    public async Task<CalendarShareDto> Handle(GrantCalendarShareCommand request, CancellationToken ct)
    {
        var viewer = await userRepository.GetByIdAsync(request.ViewerUserId, ct)
                     ?? throw new KeyNotFoundException("User not found.");

        if (await shareRepository.GetActiveShareAsync(request.OwnerUserId, request.ViewerUserId, ct) is not null)
            throw new InvalidOperationException("You have already shared your calendar with this user.");

        var share = CalendarShare.Create(request.OwnerUserId, request.ViewerUserId);
        await shareRepository.AddAsync(share, ct);

        return new CalendarShareDto(share.Id, viewer.ToDto(), share.CreatedAt);
    }
}
