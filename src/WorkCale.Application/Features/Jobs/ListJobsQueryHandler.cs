using MediatR;
using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
using WorkCale.Application.Services;

namespace WorkCale.Application.Features.Jobs;

public class ListJobsQueryHandler(IJobRepository repository)
    : IRequestHandler<ListJobsQuery, IEnumerable<JobDto>>
{
    public async Task<IEnumerable<JobDto>> Handle(ListJobsQuery request, CancellationToken ct)
    {
        var jobs = await repository.GetByUserIdAsync(request.UserId, request.IncludeArchived, ct);
        return jobs.Select(j => j.ToDto());
    }
}
