namespace WorkCale.Domain.Entities;

public class ShiftCategory
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Color { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public User User { get; private set; } = default!;
    public ICollection<Shift> Shifts { get; private set; } = [];

    private ShiftCategory() { }

    public static ShiftCategory Create(Guid userId, string name, string color)
    {
        return new ShiftCategory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Color = color,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string color)
    {
        Name = name;
        Color = color;
        UpdatedAt = DateTime.UtcNow;
    }
}
