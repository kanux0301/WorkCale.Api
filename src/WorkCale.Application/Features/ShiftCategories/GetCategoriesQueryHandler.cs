using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using MediatR;

namespace WorkCale.Application.Features.ShiftCategories;

public class GetCategoriesQueryHandler(IShiftCategoryRepository repository)
    : IRequestHandler<GetCategoriesQuery, IEnumerable<ShiftCategoryDto>>
{
    public async Task<IEnumerable<ShiftCategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var categories = await repository.GetByUserIdAsync(request.UserId, ct);
        return categories.Select(c => new ShiftCategoryDto(c.Id, c.Name, c.Color, c.CreatedAt));
    }
}
