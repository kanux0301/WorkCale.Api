using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using MediatR;

namespace WorkCale.Application.Features.Shifts;

public class UpdateShiftCommandHandler(
    IShiftRepository shiftRepository,
    IShiftCategoryRepository categoryRepository)
    : IRequestHandler<UpdateShiftCommand, ShiftDto>
{
    public async Task<ShiftDto> Handle(UpdateShiftCommand request, CancellationToken ct)
    {
        var shift = await shiftRepository.GetByIdAsync(request.ShiftId, ct)
                    ?? throw new KeyNotFoundException("Shift not found.");

        if (shift.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not own this shift.");

        var category = await categoryRepository.GetByIdAsync(request.CategoryId, ct)
                       ?? throw new KeyNotFoundException("Category not found.");

        if (category.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not own this category.");

        var start = TimeOnly.ParseExact(request.StartTime, "HH:mm");
        var end = TimeOnly.ParseExact(request.EndTime, "HH:mm");

        shift.Update(request.CategoryId, request.Date, start, end, request.Location, request.Notes);
        await shiftRepository.UpdateAsync(shift, ct);

        return new ShiftDto(
            shift.Id, shift.Date,
            shift.StartTime.ToString("HH:mm"), shift.EndTime.ToString("HH:mm"),
            shift.Location, shift.Notes, shift.CreatedAt, shift.UpdatedAt,
            new ShiftCategoryDto(category.Id, category.Name, category.Color, category.DefaultStartTime, category.DefaultEndTime, category.CreatedAt));
    }
}
