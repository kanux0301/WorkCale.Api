using WorkCale.Application.Services;
using MediatR;

namespace WorkCale.Application.Features.CalendarShares;

public class RevokeCalendarShareCommandHandler(ICalendarShareRepository repository)
    : IRequestHandler<RevokeCalendarShareCommand>
{
    public async Task Handle(RevokeCalendarShareCommand request, CancellationToken ct)
    {
        var share = await repository.GetByIdAsync(request.ShareId, ct)
                    ?? throw new KeyNotFoundException("Share not found.");

        if (share.OwnerUserId != request.OwnerUserId)
            throw new UnauthorizedAccessException("You do not own this share.");

        share.Revoke();
        await repository.UpdateAsync(share, ct);
    }
}
