using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.Jobs;

namespace WorkCale.Api.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize]
public class JobsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobDto>>> GetAll(
        [FromQuery] bool includeArchived = false,
        CancellationToken ct = default) =>
        Ok(await mediator.Send(new ListJobsQuery(this.GetUserId(), includeArchived), ct));

    [HttpPost]
    public async Task<ActionResult<JobDto>> Create([FromBody] CreateJobRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateJobCommand(
            this.GetUserId(), request.Name, request.Color, request.Icon), ct);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JobDto>> Update(Guid id, [FromBody] UpdateJobRequest request, CancellationToken ct) =>
        Ok(await mediator.Send(new UpdateJobCommand(
            id, this.GetUserId(), request.Name, request.Color, request.Icon), ct));

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ArchiveJobCommand(id, this.GetUserId()), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/make-default")]
    public async Task<IActionResult> MakeDefault(Guid id, CancellationToken ct)
    {
        await mediator.Send(new SetDefaultJobCommand(id, this.GetUserId()), ct);
        return NoContent();
    }
}
