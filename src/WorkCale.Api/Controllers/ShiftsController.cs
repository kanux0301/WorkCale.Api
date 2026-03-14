using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.Shifts;

namespace WorkCale.Api.Controllers;

[ApiController]
[Route("api/shifts")]
[Authorize]
public class ShiftsController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShiftDto>>> GetByMonth(
        [FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var result = await mediator.Send(new GetShiftsQuery(UserId, year, month), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ShiftDto>> Create([FromBody] CreateShiftRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await mediator.Send(new CreateShiftCommand(
            UserId, request.Date, request.StartTime, request.EndTime,
            request.CategoryId, request.Location, request.Notes), ct);
        return CreatedAtAction(nameof(GetByMonth), result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShiftDto>> Update(Guid id, [FromBody] UpdateShiftRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await mediator.Send(new UpdateShiftCommand(
            id, UserId, request.Date, request.StartTime, request.EndTime,
            request.CategoryId, request.Location, request.Notes), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteShiftCommand(id, UserId), ct);
        return NoContent();
    }
}
