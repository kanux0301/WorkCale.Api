using System.Security.Cryptography;
using MediatR;
using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Application.Features.Invites;

public class CreateInvitesCommandHandler(IInviteCodeRepository repo)
    : IRequestHandler<CreateInvitesCommand, CreateInvitesResponse>
{
    // Ambiguous characters (0/O, 1/I/L) removed so a code read aloud or screenshotted still decodes cleanly.
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public async Task<CreateInvitesResponse> Handle(CreateInvitesCommand request, CancellationToken ct)
    {
        DateTime? expires = request.ExpiresInHours is { } hours
            ? DateTime.UtcNow.AddHours(hours)
            : null;

        var created = new List<InviteCodeDto>(request.Count);
        for (var i = 0; i < request.Count; i++)
        {
            var invite = InviteCode.Issue(request.IssuedByUserId, GenerateCode(), expires, request.Note);
            await repo.AddAsync(invite, ct);
            created.Add(invite.ToDto());
        }

        return new CreateInvitesResponse(created);
    }

    private static string GenerateCode()
    {
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        Span<char> chars = stackalloc char[8];
        for (var i = 0; i < 8; i++)
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        return $"WC-{new string(chars[..4])}-{new string(chars[4..])}";
    }
}

public class ListInvitesQueryHandler(IInviteCodeRepository repo)
    : IRequestHandler<ListInvitesQuery, IReadOnlyList<InviteCodeDto>>
{
    public async Task<IReadOnlyList<InviteCodeDto>> Handle(ListInvitesQuery request, CancellationToken ct)
    {
        var all = await repo.ListAsync(ct);
        return all.Select(i => i.ToDto()).ToList();
    }
}
