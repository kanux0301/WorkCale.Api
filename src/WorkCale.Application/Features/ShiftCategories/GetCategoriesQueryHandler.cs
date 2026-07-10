using MediatR;
using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
using WorkCale.Application.Services;

namespace WorkCale.Application.Features.ShiftCategories;

public class GetCategoriesQueryHandler(IShiftCategoryRepository repository)
    : IRequestHandler<GetCategoriesQuery, IEnumerable<ShiftCategoryDto>>
{
    public async Task<IEnumerable<ShiftCategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var categories = await repository.GetByUserIdAsync(request.UserId, ct);
        if (request.JobId is Guid jobId)
            categories = categories.Where(c => c.JobId == jobId);
        return categories.Select(c => c.ToDto());
    }
}
