using MediatR;
using WorkCale.Application.Common;
using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Application.Features.Shifts;

public class CreateShiftCommandHandler(
    IShiftRepository shiftRepository,
    IShiftCategoryRepository categoryRepository)
    : IRequestHandler<CreateShiftCommand, ShiftDto>
{
    public async Task<ShiftDto> Handle(CreateShiftCommand request, CancellationToken ct)
    {
        var category = OwnershipGuards.RequireOwned(
            await categoryRepository.GetByIdAsync(request.CategoryId, ct),
            request.UserId, c => c.UserId, "Category");

        var shift = Shift.Create(
            request.UserId, request.CategoryId, request.Date,
            TimeFormats.ParseHHmm(request.StartTime), TimeFormats.ParseHHmm(request.EndTime),
            request.Location, request.Notes, request.UnpaidBreakMinutes);

        await shiftRepository.AddAsync(shift, ct);

        return shift.ToDto(category);
    }
}
