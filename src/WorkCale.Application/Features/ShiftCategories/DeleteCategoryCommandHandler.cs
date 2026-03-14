using WorkCale.Application.Services;
using MediatR;

namespace WorkCale.Application.Features.ShiftCategories;

public class DeleteCategoryCommandHandler(IShiftCategoryRepository repository)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await repository.GetByIdAsync(request.CategoryId, ct)
                       ?? throw new KeyNotFoundException("Category not found.");

        if (category.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not own this category.");

        if (await repository.HasShiftsAsync(request.CategoryId, ct))
            throw new InvalidOperationException("Cannot delete a category that has shifts assigned to it. Reassign or delete those shifts first.");

        await repository.DeleteAsync(category, ct);
    }
}
