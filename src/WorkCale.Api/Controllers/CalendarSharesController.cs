using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.CalendarShares;

namespace WorkCale.Api.Controllers;

[ApiController]
[Route("api/calendar-shares")]
[Authorize]
public class CalendarSharesController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("mine")]
    public async Task<ActionResult<MySharesDto>> GetMine(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMySharesQuery(UserId), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CalendarShareDto>> Grant([FromBody] GrantShareRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await mediator.Send(new GrantCalendarShareCommand(UserId, request.ViewerUserId), ct);
        return CreatedAtAction(nameof(GetMine), result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        await mediator.Send(new RevokeCalendarShareCommand(id, UserId), ct);
        return NoContent();
    }

    [HttpGet("from/{ownerUserId:guid}")]
    public async Task<ActionResult<SharedCalendarDto>> GetSharedCalendar(
        Guid ownerUserId, [FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSharedCalendarQuery(UserId, ownerUserId, year, month), ct);
        return Ok(result);
    }
}
