using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.Jobs;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.Jobs;

public class JobsHandlerTests
{
    private readonly IJobRepository _repo = Substitute.For<IJobRepository>();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task List_ReturnsMappedDtos()
    {
        var job = Job.Create(_userId, "My Job", "#4C6FA3", "briefcase-outline", isDefault: true);
        _repo.GetByUserIdAsync(_userId, false, Arg.Any<CancellationToken>())
            .Returns(new[] { job });
        var handler = new ListJobsQueryHandler(_repo);

        var result = (await handler.Handle(new ListJobsQuery(_userId), default)).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("My Job");
        result[0].IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task Create_PersistsNewJob_WithIncrementingSortOrder()
    {
        var existing = Job.Create(_userId, "First", "#111111");
        existing.SetSortOrder(3);
        _repo.GetByUserIdAsync(_userId, true, Arg.Any<CancellationToken>())
            .Returns(new[] { existing });
        var handler = new CreateJobCommandHandler(_repo);

        var dto = await handler.Handle(new CreateJobCommand(_userId, "Second", "#222222", null), default);

        dto.Name.Should().Be("Second");
        dto.SortOrder.Should().Be(4);
        await _repo.Received(1).AddAsync(
            Arg.Is<Job>(j => j.Name == "Second" && j.SortOrder == 4 && !j.IsDefault),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_MutatesOwnedJob()
    {
        var job = Job.Create(_userId, "Old", "#111111");
        _repo.GetByIdAsync(job.Id, Arg.Any<CancellationToken>()).Returns(job);
        var handler = new UpdateJobCommandHandler(_repo);

        await handler.Handle(new UpdateJobCommand(job.Id, _userId, "New", "#ABCDEF", "new-icon"), default);

        job.Name.Should().Be("New");
        job.Color.Should().Be("#ABCDEF");
        job.Icon.Should().Be("new-icon");
    }

    [Fact]
    public async Task Update_ForeignJob_ThrowsUnauthorized()
    {
        var job = Job.Create(Guid.NewGuid(), "NotYours", "#111111");
        _repo.GetByIdAsync(job.Id, Arg.Any<CancellationToken>()).Returns(job);
        var handler = new UpdateJobCommandHandler(_repo);

        Func<Task> act = () => handler.Handle(new UpdateJobCommand(job.Id, _userId, "x", "#ABCDEF", null), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Archive_DefaultJob_Throws()
    {
        var job = Job.Create(_userId, "Default", "#111111", isDefault: true);
        _repo.GetByIdAsync(job.Id, Arg.Any<CancellationToken>()).Returns(job);
        var handler = new ArchiveJobCommandHandler(_repo);

        Func<Task> act = () => handler.Handle(new ArchiveJobCommand(job.Id, _userId), default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SetDefault_ArchivedJob_Throws()
    {
        var job = Job.Create(_userId, "Old", "#111111");
        job.Archive();
        _repo.GetByIdAsync(job.Id, Arg.Any<CancellationToken>()).Returns(job);
        var handler = new SetDefaultJobCommandHandler(_repo);

        Func<Task> act = () => handler.Handle(new SetDefaultJobCommand(job.Id, _userId), default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SetDefault_ValidJob_CallsSwap()
    {
        var job = Job.Create(_userId, "A", "#111111");
        _repo.GetByIdAsync(job.Id, Arg.Any<CancellationToken>()).Returns(job);
        var handler = new SetDefaultJobCommandHandler(_repo);

        await handler.Handle(new SetDefaultJobCommand(job.Id, _userId), default);

        await _repo.Received(1).SwapDefaultAsync(_userId, job.Id, Arg.Any<CancellationToken>());
    }
}
