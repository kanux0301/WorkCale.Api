using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.ShiftCategories;

public record UpdateCategoryCommand(Guid CategoryId, Guid UserId, string Name, string Color, string? DefaultStartTime, string? DefaultEndTime) : IRequest<ShiftCategoryDto>;
