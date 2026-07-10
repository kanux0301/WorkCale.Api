using MediatR;
using WorkCale.Application.DTOs;

namespace WorkCale.Application.Features.Jobs;

public record UpdateJobCommand(Guid JobId, Guid UserId, string Name, string Color, string? Icon) : IRequest<JobDto>;
