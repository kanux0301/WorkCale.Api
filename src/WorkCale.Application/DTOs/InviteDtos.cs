using System.ComponentModel.DataAnnotations;

namespace WorkCale.Application.DTOs;

public record InviteCodeDto(
    Guid Id,
    string Code,
    DateTime IssuedAt,
    DateTime? ExpiresAt,
    DateTime? ConsumedAt,
    Guid? ConsumedByUserId,
    string? Note);

/// <summary>
/// Admin payload. <c>ExpiresInHours</c> is relative (null = no expiry). Server
/// converts to an absolute UTC timestamp at issue time.
/// </summary>
public record CreateInvitesRequest(
    [Range(1, 50)] int Count = 1,
    int? ExpiresInHours = null,
    [MaxLength(200)] string? Note = null);

public record CreateInvitesResponse(IReadOnlyList<InviteCodeDto> Created);
