using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using MediatR;

namespace WorkCale.Application.Features.Shifts;

public class GetShiftsQueryHandler(IShiftRepository repository)
    : IRequestHandler<GetShiftsQuery, IEnumerable<ShiftDto>>
{
    public async Task<IEnumerable<ShiftDto>> Handle(GetShiftsQuery request, CancellationToken ct)
    {
        var shifts = await repository.GetByUserAndMonthAsync(request.UserId, request.Year, request.Month, ct);
        return shifts.Select(s => new ShiftDto(
            s.Id,
            s.Date,
            s.StartTime.ToString("HH:mm"),
            s.EndTime.ToString("HH:mm"),
            s.Location,
            s.Notes,
            s.CreatedAt,
            s.UpdatedAt,
            new ShiftCategoryDto(s.Category.Id, s.Category.Name, s.Category.Color, s.Category.CreatedAt)));
    }
}
