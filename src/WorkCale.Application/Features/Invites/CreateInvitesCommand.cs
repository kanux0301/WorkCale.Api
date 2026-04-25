using MediatR;
using WorkCale.Application.DTOs;

namespace WorkCale.Application.Features.Invites;

public record CreateInvitesCommand(Guid IssuedByUserId, int Count, int? ExpiresInHours, string? Note)
    : IRequest<CreateInvitesResponse>;

public record ListInvitesQuery : IRequest<IReadOnlyList<InviteCodeDto>>;
