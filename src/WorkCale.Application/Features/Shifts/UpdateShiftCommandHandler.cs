using MediatR;
using WorkCale.Application.Common;
using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
using WorkCale.Application.Services;

namespace WorkCale.Application.Features.Shifts;

public class UpdateShiftCommandHandler(
    IShiftRepository shiftRepository,
    IShiftCategoryRepository categoryRepository)
    : IRequestHandler<UpdateShiftCommand, ShiftDto>
{
    public async Task<ShiftDto> Handle(UpdateShiftCommand request, CancellationToken ct)
    {
        var shift = OwnershipGuards.RequireOwned(
            await shiftRepository.GetByIdAsync(request.ShiftId, ct),
            request.UserId, s => s.UserId, "Shift");

        var category = OwnershipGuards.RequireOwned(
            await categoryRepository.GetByIdAsync(request.CategoryId, ct),
            request.UserId, c => c.UserId, "Category");

        shift.Update(
            category.JobId, request.CategoryId, request.Date,
            TimeFormats.ParseHHmm(request.StartTime), TimeFormats.ParseHHmm(request.EndTime),
            request.Location, request.Notes, request.UnpaidBreakMinutes);

        await shiftRepository.UpdateAsync(shift, ct);

        return shift.ToDto(category);
    }
}
