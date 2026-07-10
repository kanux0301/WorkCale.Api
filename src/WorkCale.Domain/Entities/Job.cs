namespace WorkCale.Domain.Entities;

public class Job
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Color { get; private set; } = default!;
    public string? Icon { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsArchived { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public User User { get; private set; } = default!;
    public ICollection<ShiftCategory> Categories { get; private set; } = [];
    public ICollection<Shift> Shifts { get; private set; } = [];

    private Job() { }

    public static Job Create(Guid userId, string name, string color, string? icon = null,
        bool isDefault = false, int sortOrder = 0)
    {
        return new Job
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Color = color,
            Icon = icon,
            IsDefault = isDefault,
            IsArchived = false,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string color, string? icon)
    {
        Name = name;
        Color = color;
        Icon = icon;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MakeDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (IsDefault)
            throw new InvalidOperationException("Cannot archive the default job. Set another job as default first.");
        IsArchived = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unarchive()
    {
        IsArchived = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
