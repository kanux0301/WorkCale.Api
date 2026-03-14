using MediatR;

namespace WorkCale.Application.Features.CalendarShares;

public record RevokeCalendarShareCommand(Guid ShareId, Guid OwnerUserId) : IRequest;
