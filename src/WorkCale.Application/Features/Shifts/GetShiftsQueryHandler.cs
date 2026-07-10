using MediatR;
using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
using WorkCale.Application.Services;

namespace WorkCale.Application.Features.Shifts;

public class GetShiftsQueryHandler(IShiftRepository repository)
    : IRequestHandler<GetShiftsQuery, IEnumerable<ShiftDto>>
{
    public async Task<IEnumerable<ShiftDto>> Handle(GetShiftsQuery request, CancellationToken ct)
    {
        var shifts = await repository.GetByUserAndMonthAsync(request.UserId, request.Year, request.Month, ct);
        if (request.JobId is Guid jobId)
            shifts = shifts.Where(s => s.JobId == jobId);
        return shifts.Select(s => s.ToDto());
    }
}
