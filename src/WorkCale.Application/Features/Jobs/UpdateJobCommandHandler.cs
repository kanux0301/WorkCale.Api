using MediatR;
using WorkCale.Application.Common;
using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
using WorkCale.Application.Services;

namespace WorkCale.Application.Features.Jobs;

public class UpdateJobCommandHandler(IJobRepository repository)
    : IRequestHandler<UpdateJobCommand, JobDto>
{
    public async Task<JobDto> Handle(UpdateJobCommand request, CancellationToken ct)
    {
        var job = OwnershipGuards.RequireOwned(
            await repository.GetByIdAsync(request.JobId, ct),
            request.UserId, j => j.UserId, "Job");

        job.Update(request.Name, request.Color, request.Icon);
        await repository.UpdateAsync(job, ct);
        return job.ToDto();
    }
}
