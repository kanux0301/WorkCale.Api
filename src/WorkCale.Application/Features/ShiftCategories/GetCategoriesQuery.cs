using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.ShiftCategories;

public record GetCategoriesQuery(Guid UserId) : IRequest<IEnumerable<ShiftCategoryDto>>;
