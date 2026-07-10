using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.Shifts;

public record GetShiftsQuery(Guid UserId, int Year, int Month, Guid? JobId = null) : IRequest<IEnumerable<ShiftDto>>;
