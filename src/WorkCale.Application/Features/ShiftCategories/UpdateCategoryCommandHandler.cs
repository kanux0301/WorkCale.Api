using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using MediatR;

namespace WorkCale.Application.Features.ShiftCategories;

public class UpdateCategoryCommandHandler(IShiftCategoryRepository repository)
    : IRequestHandler<UpdateCategoryCommand, ShiftCategoryDto>
{
    public async Task<ShiftCategoryDto> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await repository.GetByIdAsync(request.CategoryId, ct)
                       ?? throw new KeyNotFoundException("Category not found.");

        if (category.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not own this category.");

        category.Update(request.Name, request.Color, request.DefaultStartTime, request.DefaultEndTime);
        await repository.UpdateAsync(category, ct);

        return new ShiftCategoryDto(category.Id, category.Name, category.Color, category.DefaultStartTime, category.DefaultEndTime, category.CreatedAt);
    }
}
