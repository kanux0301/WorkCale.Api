using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.Auth;

namespace WorkCale.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register([FromBody] RegisterRequest request, CancellationToken ct) =>
        Ok(await mediator.Send(new RegisterCommand(request.Email, request.DisplayName, request.Password, request.InviteCode), ct));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginRequest request, CancellationToken ct) =>
        Ok(await mediator.Send(new LoginCommand(request.Email, request.Password), ct));

    [HttpPost("google")]
    public async Task<ActionResult<AuthResult>> GoogleLogin([FromBody] GoogleLoginRequest request, CancellationToken ct) =>
        Ok(await mediator.Send(new GoogleLoginCommand(request.IdToken, request.InviteCode), ct));

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResult>> Refresh([FromBody] RefreshRequest request, CancellationToken ct) =>
        Ok(await mediator.Send(new RefreshCommand(request.RefreshToken), ct));

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        await mediator.Send(new LogoutCommand(request.RefreshToken), ct);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct) =>
        Ok(await mediator.Send(new GetCurrentUserQuery(this.GetUserId()), ct));

    [Authorize]
    [HttpPatch("me")]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken ct) =>
        Ok(await mediator.Send(
            new UpdateProfileCommand(this.GetUserId(), request.DisplayName, request.AvatarColor, request.AvatarIcon),
            ct));
}
