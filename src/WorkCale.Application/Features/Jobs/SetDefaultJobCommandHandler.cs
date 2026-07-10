using MediatR;
using WorkCale.Application.Common;
using WorkCale.Application.Services;

namespace WorkCale.Application.Features.Jobs;

public class SetDefaultJobCommandHandler(IJobRepository repository)
    : IRequestHandler<SetDefaultJobCommand>
{
    public async Task Handle(SetDefaultJobCommand request, CancellationToken ct)
    {
        var job = OwnershipGuards.RequireOwned(
            await repository.GetByIdAsync(request.JobId, ct),
            request.UserId, j => j.UserId, "Job");

        if (job.IsArchived)
            throw new InvalidOperationException("Cannot make an archived job the default. Unarchive it first.");

        await repository.SwapDefaultAsync(request.UserId, request.JobId, ct);
    }
}
