using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WorkCale.Api.Controllers;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.CalendarShares;

namespace WorkCale.Api.Tests.Controllers;

public class CalendarSharesControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CalendarSharesController _sut;
    private readonly Guid _userId = Guid.NewGuid();

    private static readonly UserDto SampleUser = new(Guid.NewGuid(), "owner@x.com", "Owner", null);
    private static readonly CalendarShareDto SampleShare = new(Guid.NewGuid(), SampleUser, DateTime.UtcNow);

    private static readonly MySharesDto SampleMyShares = new(
        [SampleShare],
        []);

    public CalendarSharesControllerTests()
    {
        _sut = new CalendarSharesController(_mediator);
        _sut.ControllerContext = CreateAuthContext(_userId);
    }

    // ── GetMine ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMine_ReturnsOk()
    {
        _mediator.Send(Arg.Any<GetMySharesQuery>(), Arg.Any<CancellationToken>())
            .Returns(SampleMyShares);

        var result = await _sut.GetMine(CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMine_SendsQueryWithUserId()
    {
        _mediator.Send(Arg.Any<GetMySharesQuery>(), Arg.Any<CancellationToken>())
            .Returns(SampleMyShares);

        await _sut.GetMine(CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<GetMySharesQuery>(q => q.UserId == _userId),
            Arg.Any<CancellationToken>());
    }

    // ── Grant ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Grant_ValidRequest_ReturnsCreatedAtAction()
    {
        var viewerId = Guid.NewGuid();
        _mediator.Send(Arg.Any<GrantCalendarShareCommand>(), Arg.Any<CancellationToken>())
            .Returns(SampleShare);

        var result = await _sut.Grant(
            new GrantShareRequest(viewerId), CancellationToken.None);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Grant_EmptyViewerUserId_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("ViewerUserId", "ViewerUserId is required.");

        var result = await _sut.Grant(
            new GrantShareRequest(Guid.Empty), CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        await _mediator.DidNotReceive().Send(Arg.Any<GrantCalendarShareCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Grant_SendsCommandWithOwnerAndViewerIds()
    {
        var viewerId = Guid.NewGuid();
        _mediator.Send(Arg.Any<GrantCalendarShareCommand>(), Arg.Any<CancellationToken>())
            .Returns(SampleShare);

        await _sut.Grant(new GrantShareRequest(viewerId), CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<GrantCalendarShareCommand>(c => c.OwnerUserId == _userId && c.ViewerUserId == viewerId),
            Arg.Any<CancellationToken>());
    }

    // ── Revoke ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Revoke_ValidId_ReturnsNoContent()
    {
        _mediator.Send(Arg.Any<RevokeCalendarShareCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _sut.Revoke(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Revoke_SendsCommandWithShareIdAndUserId()
    {
        var shareId = Guid.NewGuid();
        _mediator.Send(Arg.Any<RevokeCalendarShareCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _sut.Revoke(shareId, CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<RevokeCalendarShareCommand>(c => c.ShareId == shareId && c.OwnerUserId == _userId),
            Arg.Any<CancellationToken>());
    }

    // ── GetSharedCalendar ─────────────────────────────────────────────────

    [Fact]
    public async Task GetSharedCalendar_ReturnsOk()
    {
        var ownerId = Guid.NewGuid();
        var dto = new SharedCalendarDto(SampleUser, []);
        _mediator.Send(Arg.Any<GetSharedCalendarQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);

        var result = await _sut.GetSharedCalendar(ownerId, 2026, 3, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSharedCalendar_SendsQueryWithViewerAndOwnerIds()
    {
        var ownerId = Guid.NewGuid();
        _mediator.Send(Arg.Any<GetSharedCalendarQuery>(), Arg.Any<CancellationToken>())
            .Returns(new SharedCalendarDto(SampleUser, []));

        await _sut.GetSharedCalendar(ownerId, 2026, 3, CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<GetSharedCalendarQuery>(q =>
                q.ViewerUserId == _userId &&
                q.OwnerUserId == ownerId &&
                q.Year == 2026 &&
                q.Month == 3),
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
