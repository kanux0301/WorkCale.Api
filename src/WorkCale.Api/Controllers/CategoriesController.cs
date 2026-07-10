using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.ShiftCategories;

namespace WorkCale.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShiftCategoryDto>>> GetAll(
        [FromQuery] Guid? jobId,
        CancellationToken ct) =>
        Ok(await mediator.Send(new GetCategoriesQuery(this.GetUserId(), jobId), ct));

    [HttpPost]
    public async Task<ActionResult<ShiftCategoryDto>> Create([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateCategoryCommand(
            this.GetUserId(), request.JobId, request.Name, request.Color,
            request.DefaultStartTime, request.DefaultEndTime, request.Icon), ct);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShiftCategoryDto>> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct) =>
        Ok(await mediator.Send(new UpdateCategoryCommand(
            id, this.GetUserId(), request.Name, request.Color,
            request.DefaultStartTime, request.DefaultEndTime, request.Icon), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteCategoryCommand(id, this.GetUserId()), ct);
        return NoContent();
    }
}
