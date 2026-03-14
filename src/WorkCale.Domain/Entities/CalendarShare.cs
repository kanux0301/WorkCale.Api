namespace WorkCale.Domain.Entities;

public class CalendarShare
{
    public Guid Id { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public Guid ViewerUserId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public User OwnerUser { get; private set; } = default!;
    public User ViewerUser { get; private set; } = default!;

    private CalendarShare() { }

    public static CalendarShare Create(Guid ownerUserId, Guid viewerUserId)
    {
        return new CalendarShare
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            ViewerUserId = viewerUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Revoke()
    {
        IsActive = false;
        RevokedAt = DateTime.UtcNow;
    }
}
