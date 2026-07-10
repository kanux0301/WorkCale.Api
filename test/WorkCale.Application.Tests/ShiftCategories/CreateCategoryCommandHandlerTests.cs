using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.ShiftCategories;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.ShiftCategories;

public class CreateCategoryCommandHandlerTests
{
    private readonly IShiftCategoryRepository _repo = Substitute.For<IShiftCategoryRepository>();
    private readonly IJobRepository _jobRepo = Substitute.For<IJobRepository>();
    private readonly CreateCategoryCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _jobId = Guid.NewGuid();

    public CreateCategoryCommandHandlerTests()
    {
        _handler = new CreateCategoryCommandHandler(_repo, _jobRepo);
        _repo.AddAsync(Arg.Any<ShiftCategory>(), default).Returns(Task.CompletedTask);
        _jobRepo.GetByIdAsync(_jobId, Arg.Any<CancellationToken>())
            .Returns(Job.Create(_userId, "My Job", "#4C6FA3"));
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsDto()
    {
        var command = new CreateCategoryCommand(_userId, _jobId, "Evening Shift", "#EC4899", null, null);

        var result = await _handler.Handle(command, default);

        result.Name.Should().Be("Evening Shift");
        result.Color.Should().Be("#EC4899");
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithValidData_PersistsCategory()
    {
        var command = new CreateCategoryCommand(_userId, _jobId, "Overnight", "#8B5CF6", null, null);

        await _handler.Handle(command, default);

        await _repo.Received(1).AddAsync(
            Arg.Is<ShiftCategory>(c => c.Name == "Overnight" && c.UserId == _userId && c.JobId == _jobId),
            default);
    }
}
