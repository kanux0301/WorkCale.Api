using MediatR;
using WorkCale.Application.Common;
using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Application.Features.ShiftCategories;

public class CreateCategoryCommandHandler(
    IShiftCategoryRepository repository,
    IJobRepository jobRepository)
    : IRequestHandler<CreateCategoryCommand, ShiftCategoryDto>
{
    public async Task<ShiftCategoryDto> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        OwnershipGuards.RequireOwned(
            await jobRepository.GetByIdAsync(request.JobId, ct),
            request.UserId, j => j.UserId, "Job");

        var category = ShiftCategory.Create(
            request.UserId, request.JobId, request.Name, request.Color,
            request.DefaultStartTime, request.DefaultEndTime, request.Icon);

        await repository.AddAsync(category, ct);
        return category.ToDto();
    }
}
