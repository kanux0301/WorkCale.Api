using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
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
            grantedByMe.Select(s => new CalendarShareDto(s.Id, s.ViewerUser.ToDto(), s.CreatedAt)),
            grantedToMe.Select(s => new CalendarShareDto(s.Id, s.OwnerUser.ToDto(), s.CreatedAt)));
    }
}
