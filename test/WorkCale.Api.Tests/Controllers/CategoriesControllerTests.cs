using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WorkCale.Api.Controllers;
using WorkCale.Application.DTOs;
using WorkCale.Application.Features.ShiftCategories;

namespace WorkCale.Api.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CategoriesController _sut;
    private readonly Guid _userId = Guid.NewGuid();

    private static readonly ShiftCategoryDto SampleCategory =
        new(Guid.NewGuid(), "Day Shift", "#F59E0B", DateTime.UtcNow);

    public CategoriesControllerTests()
    {
        _sut = new CategoriesController(_mediator);
        _sut.ControllerContext = CreateAuthContext(_userId);
    }

    // ── GetAll ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk_WithCategories()
    {
        var categories = new[] { SampleCategory };
        _mediator.Send(Arg.Any<GetCategoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(categories);

        var result = await _sut.GetAll(CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(categories);
    }

    [Fact]
    public async Task GetAll_SendsQueryWithUserId()
    {
        _mediator.Send(Arg.Any<GetCategoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ShiftCategoryDto>());

        await _sut.GetAll(CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<GetCategoriesQuery>(q => q.UserId == _userId),
            Arg.Any<CancellationToken>());
    }

    // ── Create ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        _mediator.Send(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(SampleCategory);

        var result = await _sut.Create(
            new CreateCategoryRequest("Day Shift", "#F59E0B"), CancellationToken.None);

        result.Result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeEquivalentTo(SampleCategory);
    }

    [Fact]
    public async Task Create_EmptyName_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("Name", "Name is required.");

        var result = await _sut.Create(
            new CreateCategoryRequest("", "#F59E0B"), CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        await _mediator.DidNotReceive().Send(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_EmptyColor_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("Color", "Color is required.");

        var result = await _sut.Create(
            new CreateCategoryRequest("Day Shift", ""), CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_SendsCommandWithUserId()
    {
        _mediator.Send(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(SampleCategory);

        await _sut.Create(new CreateCategoryRequest("Day", "#AABBCC"), CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<CreateCategoryCommand>(c => c.UserId == _userId && c.Name == "Day"),
            Arg.Any<CancellationToken>());
    }

    // ── Update ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ValidRequest_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _mediator.Send(Arg.Any<UpdateCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(SampleCategory);

        var result = await _sut.Update(id,
            new UpdateCategoryRequest("Night Shift", "#6366F1"), CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_EmptyName_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("Name", "Name is required.");

        var result = await _sut.Update(Guid.NewGuid(),
            new UpdateCategoryRequest("", "#6366F1"), CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_EmptyColor_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("Color", "Color is required.");

        var result = await _sut.Update(Guid.NewGuid(),
            new UpdateCategoryRequest("Night", ""), CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_SendsCommandWithIdAndUserId()
    {
        var id = Guid.NewGuid();
        _mediator.Send(Arg.Any<UpdateCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(SampleCategory);

        await _sut.Update(id, new UpdateCategoryRequest("New", "#AABBCC"), CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<UpdateCategoryCommand>(c => c.CategoryId == id && c.UserId == _userId),
            Arg.Any<CancellationToken>());
    }

    // ── Delete ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ValidId_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _mediator.Send(Arg.Any<DeleteCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _sut.Delete(id, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_SendsCommandWithIdAndUserId()
    {
        var id = Guid.NewGuid();
        _mediator.Send(Arg.Any<DeleteCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _sut.Delete(id, CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<DeleteCategoryCommand>(c => c.CategoryId == id && c.UserId == _userId),
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
