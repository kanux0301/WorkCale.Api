namespace WorkCale.Domain.Entities;

public class Shift
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public string? Location { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public User User { get; private set; } = default!;
    public ShiftCategory Category { get; private set; } = default!;

    private Shift() { }

    public static Shift Create(Guid userId, Guid categoryId, DateOnly date,
        TimeOnly startTime, TimeOnly endTime, string? location, string? notes)
    {
        return new Shift
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = categoryId,
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            Location = location,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(Guid categoryId, DateOnly date, TimeOnly startTime,
        TimeOnly endTime, string? location, string? notes)
    {
        CategoryId = categoryId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Location = location;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTimes(TimeOnly startTime, TimeOnly endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
        UpdatedAt = DateTime.UtcNow;
    }
}
