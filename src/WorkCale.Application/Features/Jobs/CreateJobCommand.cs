using MediatR;
using WorkCale.Application.DTOs;

namespace WorkCale.Application.Features.Jobs;

public record CreateJobCommand(Guid UserId, string Name, string Color, string? Icon) : IRequest<JobDto>;
