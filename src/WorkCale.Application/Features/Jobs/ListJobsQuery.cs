using MediatR;
using WorkCale.Application.DTOs;

namespace WorkCale.Application.Features.Jobs;

public record ListJobsQuery(Guid UserId, bool IncludeArchived = false) : IRequest<IEnumerable<JobDto>>;
