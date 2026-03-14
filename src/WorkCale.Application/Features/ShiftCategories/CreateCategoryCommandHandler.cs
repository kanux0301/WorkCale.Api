using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using MediatR;

namespace WorkCale.Application.Features.ShiftCategories;

public class CreateCategoryCommandHandler(IShiftCategoryRepository repository)
    : IRequestHandler<CreateCategoryCommand, ShiftCategoryDto>
{
    public async Task<ShiftCategoryDto> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var category = ShiftCategory.Create(request.UserId, request.Name, request.Color, request.DefaultStartTime, request.DefaultEndTime);
        await repository.AddAsync(category, ct);
        return new ShiftCategoryDto(category.Id, category.Name, category.Color, category.DefaultStartTime, category.DefaultEndTime, category.CreatedAt);
    }
}
