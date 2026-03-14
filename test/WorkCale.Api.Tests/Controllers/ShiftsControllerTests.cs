using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WorkCale.Api.Controllers;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.Shifts;

namespace WorkCale.Api.Tests.Controllers;

public class ShiftsControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ShiftsController _sut;
    private readonly Guid _userId = Guid.NewGuid();

    private static readonly ShiftCategoryDto SampleCat = new(Guid.NewGuid(), "Day", "#F59E0B", null, null, DateTime.UtcNow);
    private static readonly ShiftDto SampleShift = new(
        Guid.NewGuid(), new DateOnly(2026, 3, 15),
        "09:00", "17:00", null, null,
        DateTime.UtcNow, DateTime.UtcNow, SampleCat);

    public ShiftsControllerTests()
    {
        _sut = new ShiftsController(_mediator);
        _sut.ControllerContext = CreateAuthContext(_userId);
    }

    // ── GetByMonth ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByMonth_ReturnsOk()
    {
        _mediator.Send(Arg.Any<GetShiftsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] { SampleShift });

        var result = await _sut.GetByMonth(2026, 3, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByMonth_SendsQueryWithUserId()
    {
        _mediator.Send(Arg.Any<GetShiftsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ShiftDto>());

        await _sut.GetByMonth(2026, 3, CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<GetShiftsQuery>(q => q.UserId == _userId && q.Year == 2026 && q.Month == 3),
            Arg.Any<CancellationToken>());
    }

    // ── Create ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        _mediator.Send(Arg.Any<CreateShiftCommand>(), Arg.Any<CancellationToken>())
            .Returns(SampleShift);

        var result = await _sut.Create(
            new CreateShiftRequest(new DateOnly(2026, 3, 15), "09:00", "17:00",
                Guid.NewGuid(), null, null), CancellationToken.None);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_EmptyStartTime_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("StartTime", "StartTime is required.");

        var result = await _sut.Create(
            new CreateShiftRequest(new DateOnly(2026, 3, 15), "", "17:00",
                Guid.NewGuid(), null, null), CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        await _mediator.DidNotReceive().Send(Arg.Any<CreateShiftCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_EmptyEndTime_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("EndTime", "EndTime is required.");

        var result = await _sut.Create(
            new CreateShiftRequest(new DateOnly(2026, 3, 15), "09:00", "",
                Guid.NewGuid(), null, null), CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_EmptyCategoryId_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("CategoryId", "CategoryId is required.");

        var result = await _sut.Create(
            new CreateShiftRequest(new DateOnly(2026, 3, 15), "09:00", "17:00",
                Guid.Empty, null, null), CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_SendsCommandWithUserId()
    {
        var catId = Guid.NewGuid();
        _mediator.Send(Arg.Any<CreateShiftCommand>(), Arg.Any<CancellationToken>())
            .Returns(SampleShift);

        await _sut.Create(
            new CreateShiftRequest(new DateOnly(2026, 3, 15), "09:00", "17:00",
                catId, "Office", null), CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<CreateShiftCommand>(c => c.UserId == _userId && c.CategoryId == catId),
            Arg.Any<CancellationToken>());
    }

    // ── Update ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ValidRequest_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _mediator.Send(Arg.Any<UpdateShiftCommand>(), Arg.Any<CancellationToken>())
            .Returns(SampleShift);

        var result = await _sut.Update(id,
            new UpdateShiftRequest(new DateOnly(2026, 3, 15), "09:00", "17:00",
                Guid.NewGuid(), null, null), CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_EmptyStartTime_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("StartTime", "StartTime is required.");

        var result = await _sut.Update(Guid.NewGuid(),
            new UpdateShiftRequest(new DateOnly(2026, 3, 15), "", "17:00",
                Guid.NewGuid(), null, null), CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_EmptyEndTime_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("EndTime", "EndTime is required.");

        var result = await _sut.Update(Guid.NewGuid(),
            new UpdateShiftRequest(new DateOnly(2026, 3, 15), "09:00", "",
                Guid.NewGuid(), null, null), CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_SendsCommandWithShiftIdAndUserId()
    {
        var id = Guid.NewGuid();
        _mediator.Send(Arg.Any<UpdateShiftCommand>(), Arg.Any<CancellationToken>())
            .Returns(SampleShift);

        await _sut.Update(id,
            new UpdateShiftRequest(new DateOnly(2026, 3, 15), "09:00", "17:00",
                Guid.NewGuid(), null, null), CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<UpdateShiftCommand>(c => c.ShiftId == id && c.UserId == _userId),
            Arg.Any<CancellationToken>());
    }

    // ── Delete ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ValidId_ReturnsNoContent()
    {
        _mediator.Send(Arg.Any<DeleteShiftCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _sut.Delete(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_SendsCommandWithIdAndUserId()
    {
        var id = Guid.NewGuid();
        _mediator.Send(Arg.Any<DeleteShiftCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _sut.Delete(id, CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<DeleteShiftCommand>(c => c.ShiftId == id && c.UserId == _userId),
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
