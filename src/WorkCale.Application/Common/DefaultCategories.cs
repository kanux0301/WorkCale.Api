using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Application.Common;

public static class DefaultCategories
{
    public static readonly (string Name, string Color, string Start, string End)[] Items =
    [
        ("Day Shift", "#F59E0B", "08:00", "16:00"),
        ("Night Shift", "#6366F1", "20:00", "04:00"),
    ];

    public static async Task SeedAsync(IShiftCategoryRepository repository, Guid userId, Guid jobId, CancellationToken ct)
    {
        foreach (var (name, color, start, end) in Items)
            await repository.AddAsync(ShiftCategory.Create(userId, jobId, name, color, start, end), ct);
    }
}
