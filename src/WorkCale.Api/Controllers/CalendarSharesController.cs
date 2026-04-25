using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.CalendarShares;

namespace WorkCale.Api.Controllers;

[ApiController]
[Route("api/calendar-shares")]
[Authorize]
public class CalendarSharesController(IMediator mediator) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<ActionResult<MySharesDto>> GetMine(CancellationToken ct) =>
        Ok(await mediator.Send(new GetMySharesQuery(this.GetUserId()), ct));

    [HttpPost]
    public async Task<ActionResult<CalendarShareDto>> Grant([FromBody] GrantShareRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new GrantCalendarShareCommand(this.GetUserId(), request.ViewerUserId), ct);
        return CreatedAtAction(nameof(GetMine), result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        await mediator.Send(new RevokeCalendarShareCommand(id, this.GetUserId()), ct);
        return NoContent();
    }

    [HttpGet("from/{ownerUserId:guid}")]
    public async Task<ActionResult<SharedCalendarDto>> GetSharedCalendar(
        Guid ownerUserId, [FromQuery] int year, [FromQuery] int month, CancellationToken ct) =>
        Ok(await mediator.Send(new GetSharedCalendarQuery(this.GetUserId(), ownerUserId, year, month), ct));
}
