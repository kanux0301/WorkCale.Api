using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using MediatR;

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

        var existing = await shareRepository.GetActiveShareAsync(request.OwnerUserId, request.ViewerUserId, ct);
        if (existing is not null)
            throw new InvalidOperationException("You have already shared your calendar with this user.");

        var share = CalendarShare.Create(request.OwnerUserId, request.ViewerUserId);
        await shareRepository.AddAsync(share, ct);

        var viewerDto = new UserDto(viewer.Id, viewer.Email, viewer.DisplayName, viewer.AvatarUrl);
        return new CalendarShareDto(share.Id, viewerDto, share.CreatedAt);
    }
}
