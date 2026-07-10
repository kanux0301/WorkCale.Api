using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Application.Common;

public static class DefaultJob
{
    public const string Name = "My Job";
    public const string Color = "#4C6FA3";
    public const string Icon = "briefcase-outline";

    public static async Task<Job> SeedAsync(IJobRepository repository, Guid userId, CancellationToken ct)
    {
        var job = Job.Create(userId, Name, Color, Icon, isDefault: true, sortOrder: 0);
        await repository.AddAsync(job, ct);
        return job;
    }
}
