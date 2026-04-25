using MediatR;
using WorkCale.Application.Common;
using WorkCale.Application.Services;

namespace WorkCale.Application.Features.ShiftCategories;

public class DeleteCategoryCommandHandler(IShiftCategoryRepository repository)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = OwnershipGuards.RequireOwned(
            await repository.GetByIdAsync(request.CategoryId, ct),
            request.UserId, c => c.UserId, "Category");

        if (await repository.HasShiftsAsync(request.CategoryId, ct))
            throw new InvalidOperationException(
                "Cannot delete a category that has shifts assigned to it. Reassign or delete those shifts first.");

        await repository.DeleteAsync(category, ct);
    }
}
