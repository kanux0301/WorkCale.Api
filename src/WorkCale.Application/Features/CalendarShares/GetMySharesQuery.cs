using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.CalendarShares;

public record GetMySharesQuery(Guid UserId) : IRequest<MySharesDto>;
