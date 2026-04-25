using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.Users;

namespace WorkCale.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<UserDto>>> Search([FromQuery] string q, CancellationToken ct) =>
        Ok(await mediator.Send(new SearchUsersQuery(this.GetUserId(), q ?? string.Empty), ct));
}
