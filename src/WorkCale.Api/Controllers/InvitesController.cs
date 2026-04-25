using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.Invites;

namespace WorkCale.Api.Controllers;

/// <summary>
/// Admin-only invite-code management. The JWT's role=admin claim gates access;
/// the claim is only present for users flagged IsAdmin=true in the database.
/// </summary>
[ApiController]
[Route("api/invites")]
[Authorize(Roles = "admin")]
public class InvitesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateInvitesResponse>> Create([FromBody] CreateInvitesRequest request, CancellationToken ct) =>
        Ok(await mediator.Send(
            new CreateInvitesCommand(this.GetUserId(), request.Count, request.ExpiresInHours, request.Note), ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InviteCodeDto>>> List(CancellationToken ct) =>
        Ok(await mediator.Send(new ListInvitesQuery(), ct));
}
