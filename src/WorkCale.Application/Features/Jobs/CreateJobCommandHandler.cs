using MediatR;
using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Application.Features.Jobs;

public class CreateJobCommandHandler(IJobRepository repository)
    : IRequestHandler<CreateJobCommand, JobDto>
{
    public async Task<JobDto> Handle(CreateJobCommand request, CancellationToken ct)
    {
        var existing = await repository.GetByUserIdAsync(request.UserId, includeArchived: true, ct);
        var nextSort = existing.Any() ? existing.Max(j => j.SortOrder) + 1 : 0;

        var job = Job.Create(request.UserId, request.Name, request.Color, request.Icon, isDefault: false, sortOrder: nextSort);
        await repository.AddAsync(job, ct);
        return job.ToDto();
    }
}
