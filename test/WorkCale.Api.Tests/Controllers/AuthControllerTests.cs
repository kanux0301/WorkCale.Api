using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WorkCale.Api.Controllers;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.Auth;

namespace WorkCale.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly AuthController _sut;

    private static readonly UserDto SampleUser = new(Guid.NewGuid(), "a@b.com", "Alice", null);
    private static readonly AuthResult SampleAuth = new("access", "refresh", SampleUser);

    public AuthControllerTests()
    {
        _sut = new AuthController(_mediator);
    }

    // Register
    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        _mediator.Send(Arg.Any<RegisterCommand>(), Arg.Any<CancellationToken>()).Returns(SampleAuth);
        var result = await _sut.Register(new RegisterRequest("a@b.com", "Alice", "Password1!", "WC-TEST-0000"), CancellationToken.None);
        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(SampleAuth);
    }

    [Fact]
    public async Task Register_SendsCorrectCommand()
    {
        _mediator.Send(Arg.Any<RegisterCommand>(), Arg.Any<CancellationToken>()).Returns(SampleAuth);
        await _sut.Register(new RegisterRequest("alice@x.com", "Alice", "pass", "WC-TEST-0000"), CancellationToken.None);
        await _mediator.Received(1).Send(
            Arg.Is<RegisterCommand>(c => c.Email == "alice@x.com" && c.DisplayName == "Alice" && c.Password == "pass" && c.InviteCode == "WC-TEST-0000"),
            Arg.Any<CancellationToken>());
    }

    // Login
    [Fact]
    public async Task Login_ValidRequest_ReturnsOk()
    {
        _mediator.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>()).Returns(SampleAuth);
        var result = await _sut.Login(new LoginRequest("a@b.com", "password"), CancellationToken.None);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // GoogleLogin
    [Fact]
    public async Task GoogleLogin_ValidRequest_ReturnsOk()
    {
        _mediator.Send(Arg.Any<GoogleLoginCommand>(), Arg.Any<CancellationToken>()).Returns(SampleAuth);
        var result = await _sut.GoogleLogin(new GoogleLoginRequest("google-id-token"), CancellationToken.None);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // Refresh
    [Fact]
    public async Task Refresh_ValidRequest_ReturnsOk()
    {
        _mediator.Send(Arg.Any<RefreshCommand>(), Arg.Any<CancellationToken>()).Returns(SampleAuth);
        var result = await _sut.Refresh(new RefreshRequest("refresh-token"), CancellationToken.None);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // Logout
    [Fact]
    public async Task Logout_ValidRequest_ReturnsNoContent()
    {
        _mediator.Send(Arg.Any<LogoutCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var result = await _sut.Logout(new LogoutRequest("refresh-token"), CancellationToken.None);
        result.Should().BeOfType<NoContentResult>();
    }

    // Me
    [Fact]
    public async Task Me_Authenticated_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        _sut.ControllerContext = CreateAuthContext(userId);
        _mediator.Send(Arg.Any<GetCurrentUserQuery>(), Arg.Any<CancellationToken>()).Returns(SampleUser);
        var result = await _sut.Me(CancellationToken.None);
        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(SampleUser);
    }

    [Fact]
    public async Task Me_SendsQueryWithCorrectUserId()
    {
        var userId = Guid.NewGuid();
        _sut.ControllerContext = CreateAuthContext(userId);
        _mediator.Send(Arg.Any<GetCurrentUserQuery>(), Arg.Any<CancellationToken>()).Returns(SampleUser);
        await _sut.Me(CancellationToken.None);
        await _mediator.Received(1).Send(
            Arg.Is<GetCurrentUserQuery>(q => q.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    private static ControllerContext CreateAuthContext(Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "Test");
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
    }
}
