using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using MediatR;

namespace WorkCale.Application.Features.CalendarShares;

public class GetMySharesQueryHandler(ICalendarShareRepository repository)
    : IRequestHandler<GetMySharesQuery, MySharesDto>
{
    public async Task<MySharesDto> Handle(GetMySharesQuery request, CancellationToken ct)
    {
        var grantedByMe = await repository.GetGrantedByUserAsync(request.UserId, ct);
        var grantedToMe = await repository.GetGrantedToUserAsync(request.UserId, ct);

        return new MySharesDto(
            grantedByMe.Select(s => new CalendarShareDto(
                s.Id,
                new UserDto(s.ViewerUser.Id, s.ViewerUser.Email, s.ViewerUser.DisplayName, s.ViewerUser.AvatarUrl),
                s.CreatedAt)),
            grantedToMe.Select(s => new CalendarShareDto(
                s.Id,
                new UserDto(s.OwnerUser.Id, s.OwnerUser.Email, s.OwnerUser.DisplayName, s.OwnerUser.AvatarUrl),
                s.CreatedAt)));
    }
}
