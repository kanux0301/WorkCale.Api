using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.Users;

namespace WorkCale.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<UserDto>>> Search([FromQuery] string q, CancellationToken ct)
    {
        var result = await mediator.Send(new SearchUsersQuery(UserId, q ?? string.Empty), ct);
        return Ok(result);
    }
}
