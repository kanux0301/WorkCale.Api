using MediatR;
using WorkCale.Application.Common;
using WorkCale.Application.Services;

namespace WorkCale.Application.Features.Jobs;

public class ArchiveJobCommandHandler(IJobRepository repository)
    : IRequestHandler<ArchiveJobCommand>
{
    public async Task Handle(ArchiveJobCommand request, CancellationToken ct)
    {
        var job = OwnershipGuards.RequireOwned(
            await repository.GetByIdAsync(request.JobId, ct),
            request.UserId, j => j.UserId, "Job");

        job.Archive();
        await repository.UpdateAsync(job, ct);
    }
}
