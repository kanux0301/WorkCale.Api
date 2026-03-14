using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.Shifts;

public record CreateShiftCommand(
    Guid UserId,
    DateOnly Date,
    string StartTime,
    string EndTime,
    Guid CategoryId,
    string? Location,
    string? Notes) : IRequest<ShiftDto>;
