using WorkCale.Application.Services;
using MediatR;

namespace WorkCale.Application.Features.Shifts;

public class DeleteShiftCommandHandler(IShiftRepository repository)
    : IRequestHandler<DeleteShiftCommand>
{
    public async Task Handle(DeleteShiftCommand request, CancellationToken ct)
    {
        var shift = await repository.GetByIdAsync(request.ShiftId, ct)
                    ?? throw new KeyNotFoundException("Shift not found.");

        if (shift.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not own this shift.");

        await repository.DeleteAsync(shift, ct);
    }
}
