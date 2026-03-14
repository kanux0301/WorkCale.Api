using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.CalendarShares;

public record GrantCalendarShareCommand(Guid OwnerUserId, Guid ViewerUserId) : IRequest<CalendarShareDto>;
