using MediatR;
using WorkCale.Application.Common;
using WorkCale.Application.Services;

namespace WorkCale.Application.Features.CalendarShares;

public class RevokeCalendarShareCommandHandler(ICalendarShareRepository repository)
    : IRequestHandler<RevokeCalendarShareCommand>
{
    public async Task Handle(RevokeCalendarShareCommand request, CancellationToken ct)
    {
        var share = OwnershipGuards.RequireOwned(
            await repository.GetByIdAsync(request.ShareId, ct),
            request.OwnerUserId, s => s.OwnerUserId, "Share");

        share.Revoke();
        await repository.UpdateAsync(share, ct);
    }
}
