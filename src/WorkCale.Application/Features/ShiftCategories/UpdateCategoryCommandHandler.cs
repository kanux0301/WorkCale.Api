using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using MediatR;

namespace WorkCale.Application.Features.ShiftCategories;

public class UpdateCategoryCommandHandler(IShiftCategoryRepository repository, IShiftRepository shiftRepository)
    : IRequestHandler<UpdateCategoryCommand, ShiftCategoryDto>
{
    public async Task<ShiftCategoryDto> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await repository.GetByIdAsync(request.CategoryId, ct)
                       ?? throw new KeyNotFoundException("Category not found.");

        if (category.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not own this category.");

        var oldStart = category.DefaultStartTime;
        var oldEnd = category.DefaultEndTime;

        category.Update(request.Name, request.Color, request.DefaultStartTime, request.DefaultEndTime);
        await repository.UpdateAsync(category, ct);

        if (category.DefaultStartTime is not null && category.DefaultEndTime is not null &&
            (category.DefaultStartTime != oldStart || category.DefaultEndTime != oldEnd))
        {
            var startTime = TimeOnly.Parse(category.DefaultStartTime);
            var endTime = TimeOnly.Parse(category.DefaultEndTime);
            await shiftRepository.UpdateTimesByCategoryAsync(category.Id, startTime, endTime, ct);
        }

        return new ShiftCategoryDto(category.Id, category.Name, category.Color, category.DefaultStartTime, category.DefaultEndTime, category.CreatedAt);
    }
}
