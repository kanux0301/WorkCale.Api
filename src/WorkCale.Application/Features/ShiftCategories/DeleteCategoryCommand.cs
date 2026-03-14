using MediatR;

namespace WorkCale.Application.Features.ShiftCategories;

public record DeleteCategoryCommand(Guid CategoryId, Guid UserId) : IRequest;
