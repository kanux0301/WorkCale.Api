using MediatR;
using WorkCale.Application.Common;
using WorkCale.Application.Services;

namespace WorkCale.Application.Features.Shifts;

public class DeleteShiftCommandHandler(IShiftRepository repository)
    : IRequestHandler<DeleteShiftCommand>
{
    public async Task Handle(DeleteShiftCommand request, CancellationToken ct)
    {
        var shift = OwnershipGuards.RequireOwned(
            await repository.GetByIdAsync(request.ShiftId, ct),
            request.UserId, s => s.UserId, "Shift");

        await repository.DeleteAsync(shift, ct);
    }
}
