namespace WorkCale.Domain.Entities;

/// <summary>
/// One-time registration token. An admin issues them; Register / first-time
/// Google login consume them. Gates who can sign up on the public API so the
/// free-tier database stays bounded.
/// </summary>
public class InviteCode
{
    public Guid Id { get; private set; }

    /// <summary>Human-friendly token like "WC-AB12-CD34". Unique, compared case-insensitively at lookup.</summary>
    public string Code { get; private set; } = default!;

    public Guid IssuedByUserId { get; private set; }
    public User IssuedByUser { get; private set; } = default!;
    public DateTime IssuedAt { get; private set; }

    /// <summary>Set when someone registers with this code. Null = still redeemable.</summary>
    public Guid? ConsumedByUserId { get; private set; }
    public DateTime? ConsumedAt { get; private set; }

    /// <summary>Null = no expiry.</summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>Freeform admin label ("for Bob", "beta waitlist #3"). Optional.</summary>
    public string? Note { get; private set; }

    private InviteCode() { }

    public static InviteCode Issue(Guid issuedByUserId, string code, DateTime? expiresAt, string? note)
    {
        return new InviteCode
        {
            Id = Guid.NewGuid(),
            Code = code,
            IssuedByUserId = issuedByUserId,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            Note = note
        };
    }

    public bool IsRedeemable(DateTime nowUtc)
        => ConsumedByUserId is null && (ExpiresAt is null || ExpiresAt > nowUtc);

    public void Consume(Guid userId, DateTime nowUtc)
    {
        if (ConsumedByUserId is not null)
            throw new InvalidOperationException("Invite code already used.");
        ConsumedByUserId = userId;
        ConsumedAt = nowUtc;
    }
}
