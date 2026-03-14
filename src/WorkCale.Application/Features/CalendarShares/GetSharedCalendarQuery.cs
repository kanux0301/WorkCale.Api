using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.CalendarShares;

public record GetSharedCalendarQuery(Guid ViewerUserId, Guid OwnerUserId, int Year, int Month) : IRequest<SharedCalendarDto>;
