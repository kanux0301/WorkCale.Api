using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.Shifts;

namespace WorkCale.Api.Controllers;

[ApiController]
[Route("api/shifts")]
[Authorize]
public class ShiftsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShiftDto>>> GetByMonth(
        [FromQuery] int year, [FromQuery] int month, [FromQuery] Guid? jobId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetShiftsQuery(this.GetUserId(), year, month, jobId), ct));

    [HttpPost]
    public async Task<ActionResult<ShiftDto>> Create([FromBody] CreateShiftRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateShiftCommand(
            this.GetUserId(), request.Date, request.StartTime, request.EndTime,
            request.CategoryId, request.Location, request.Notes, request.UnpaidBreakMinutes ?? 0), ct);
        return CreatedAtAction(nameof(GetByMonth), result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShiftDto>> Update(Guid id, [FromBody] UpdateShiftRequest request, CancellationToken ct) =>
        Ok(await mediator.Send(new UpdateShiftCommand(
            id, this.GetUserId(), request.Date, request.StartTime, request.EndTime,
            request.CategoryId, request.Location, request.Notes, request.UnpaidBreakMinutes ?? 0), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteShiftCommand(id, this.GetUserId()), ct);
        return NoContent();
    }
}
