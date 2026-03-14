using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using MediatR;

namespace WorkCale.Application.Features.Shifts;

public class CreateShiftCommandHandler(
    IShiftRepository shiftRepository,
    IShiftCategoryRepository categoryRepository)
    : IRequestHandler<CreateShiftCommand, ShiftDto>
{
    public async Task<ShiftDto> Handle(CreateShiftCommand request, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, ct)
                       ?? throw new KeyNotFoundException("Category not found.");

        if (category.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not own this category.");

        var start = TimeOnly.ParseExact(request.StartTime, "HH:mm");
        var end = TimeOnly.ParseExact(request.EndTime, "HH:mm");

        var shift = Shift.Create(request.UserId, request.CategoryId, request.Date, start, end, request.Location, request.Notes);
        await shiftRepository.AddAsync(shift, ct);

        return new ShiftDto(
            shift.Id, shift.Date,
            shift.StartTime.ToString("HH:mm"), shift.EndTime.ToString("HH:mm"),
            shift.Location, shift.Notes, shift.CreatedAt, shift.UpdatedAt,
            new ShiftCategoryDto(category.Id, category.Name, category.Color, category.DefaultStartTime, category.DefaultEndTime, category.CreatedAt));
    }
}
