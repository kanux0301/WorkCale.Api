using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.ShiftCategories;

namespace WorkCale.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShiftCategoryDto>>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoriesQuery(UserId), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ShiftCategoryDto>> Create([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await mediator.Send(new CreateCategoryCommand(UserId, request.Name, request.Color, request.DefaultStartTime, request.DefaultEndTime), ct);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShiftCategoryDto>> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await mediator.Send(new UpdateCategoryCommand(id, UserId, request.Name, request.Color, request.DefaultStartTime, request.DefaultEndTime), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteCategoryCommand(id, UserId), ct);
        return NoContent();
    }
}
