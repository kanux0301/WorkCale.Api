using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.ShiftCategories;

public record CreateCategoryCommand(Guid UserId, string Name, string Color) : IRequest<ShiftCategoryDto>;
