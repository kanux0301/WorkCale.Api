using MediatR;
using WorkCale.Application.Common;
using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
using WorkCale.Application.Services;

namespace WorkCale.Application.Features.ShiftCategories;

public class UpdateCategoryCommandHandler(IShiftCategoryRepository repository, IShiftRepository shiftRepository)
    : IRequestHandler<UpdateCategoryCommand, ShiftCategoryDto>
{
    public async Task<ShiftCategoryDto> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = OwnershipGuards.RequireOwned(
            await repository.GetByIdAsync(request.CategoryId, ct),
            request.UserId, c => c.UserId, "Category");

        var (oldStart, oldEnd) = (category.DefaultStartTime, category.DefaultEndTime);

        category.Update(request.Name, request.Color, request.DefaultStartTime, request.DefaultEndTime, request.Icon);
        await repository.UpdateAsync(category, ct);

        if (category.DefaultStartTime is { } newStart && category.DefaultEndTime is { } newEnd &&
            (newStart != oldStart || newEnd != oldEnd))
        {
            await shiftRepository.UpdateTimesByCategoryAsync(
                category.Id, TimeFormats.ParseHHmm(newStart), TimeFormats.ParseHHmm(newEnd), ct);
        }

        return category.ToDto();
    }
}
