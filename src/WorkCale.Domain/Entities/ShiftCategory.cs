namespace WorkCale.Domain.Entities;

public class ShiftCategory
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Color { get; private set; } = default!;
    public string? DefaultStartTime { get; private set; }
    public string? DefaultEndTime { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public User User { get; private set; } = default!;
    public ICollection<Shift> Shifts { get; private set; } = [];

    private ShiftCategory() { }

    public static ShiftCategory Create(Guid userId, string name, string color,
        string? defaultStartTime = null, string? defaultEndTime = null)
    {
        return new ShiftCategory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Color = color,
            DefaultStartTime = defaultStartTime,
            DefaultEndTime = defaultEndTime,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string color, string? defaultStartTime, string? defaultEndTime)
    {
        Name = name;
        Color = color;
        DefaultStartTime = defaultStartTime;
        DefaultEndTime = defaultEndTime;
        UpdatedAt = DateTime.UtcNow;
    }
}
