using MediatR;
using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Application.Features.ShiftCategories;

public class CreateCategoryCommandHandler(IShiftCategoryRepository repository)
    : IRequestHandler<CreateCategoryCommand, ShiftCategoryDto>
{
    public async Task<ShiftCategoryDto> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var category = ShiftCategory.Create(
            request.UserId, request.Name, request.Color,
            request.DefaultStartTime, request.DefaultEndTime, request.Icon);

        await repository.AddAsync(category, ct);
        return category.ToDto();
    }
}
