using MediatR;

namespace WorkCale.Application.Features.Shifts;

public record DeleteShiftCommand(Guid ShiftId, Guid UserId) : IRequest;
