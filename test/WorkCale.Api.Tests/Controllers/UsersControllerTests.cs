using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WorkCale.Api.Controllers;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.Users;

namespace WorkCale.Api.Tests.Controllers;

public class UsersControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly UsersController _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public UsersControllerTests()
    {
        _sut = new UsersController(_mediator);
        _sut.ControllerContext = CreateAuthContext(_userId);
    }

    // ── Search ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Search_ReturnsOk_WithResults()
    {
        var users = new[] { new UserDto(Guid.NewGuid(), "alice@x.com", "Alice", null) };
        _mediator.Send(Arg.Any<SearchUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(users);

        var result = await _sut.Search("alice", CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task Search_ReturnsOk_WithEmptyResults()
    {
        _mediator.Send(Arg.Any<SearchUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<UserDto>());

        var result = await _sut.Search("zzznobody", CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(Array.Empty<UserDto>());
    }

    [Fact]
    public async Task Search_SendsQueryWithUserIdAndTerm()
    {
        _mediator.Send(Arg.Any<SearchUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<UserDto>());

        await _sut.Search("bob", CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<SearchUsersQuery>(q => q.RequestingUserId == _userId && q.Query == "bob"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Search_NullQuery_SendsEmptyString()
    {
        _mediator.Send(Arg.Any<SearchUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<UserDto>());

        await _sut.Search(null!, CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<SearchUsersQuery>(q => q.Query == string.Empty),
            Arg.Any<CancellationToken>());
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static ControllerContext CreateAuthContext(Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "Test");
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }
}
