namespace WorkCale.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string? PasswordHash { get; private set; }
    public string? GoogleId { get; private set; }
    public string? AvatarUrl { get; private set; }
    /// <summary>User's chosen accent color (7-char hex, e.g. "#4C6FA3"). Null = use brand default.</summary>
    public string? AvatarColor { get; private set; }
    /// <summary>Ionicon name (e.g. "briefcase-outline") for the personal avatar.</summary>
    public string? AvatarIcon { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<ShiftCategory> ShiftCategories { get; private set; } = [];
    public ICollection<Shift> Shifts { get; private set; } = [];
    public ICollection<CalendarShare> SharesGranted { get; private set; } = [];
    public ICollection<CalendarShare> SharesReceived { get; private set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    private User() { }

    public static User Create(string email, string displayName, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            DisplayName = displayName,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static User CreateWithGoogle(string email, string displayName, string googleId, string? avatarUrl)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            DisplayName = displayName,
            GoogleId = googleId,
            AvatarUrl = avatarUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void LinkGoogle(string googleId, string? avatarUrl)
    {
        GoogleId = googleId;
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string displayName, string? avatarColor = null, string? avatarIcon = null)
    {
        DisplayName = displayName;
        // Null = keep current (partial updates from DTO); empty string = explicit reset.
        if (avatarColor != null)
            AvatarColor = string.IsNullOrWhiteSpace(avatarColor) ? null : avatarColor;
        if (avatarIcon != null)
            AvatarIcon = string.IsNullOrWhiteSpace(avatarIcon) ? null : avatarIcon;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }
}
