using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.Users;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.Users;

public class SearchUsersQueryHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly SearchUsersQueryHandler _handler;

    public SearchUsersQueryHandlerTests()
    {
        _handler = new SearchUsersQueryHandler(_userRepo);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ReturnsMappedUsers()
    {
        var requestingUserId = Guid.NewGuid();
        var match = User.Create("jane@test.com", "Jane Smith", "hash");
        _userRepo.SearchAsync("jane", default).Returns([match]);

        var result = (await _handler.Handle(
            new SearchUsersQuery(requestingUserId, "jane"), default)).ToList();

        result.Should().HaveCount(1);
        result[0].Email.Should().Be("jane@test.com");
        result[0].DisplayName.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task Handle_ExcludesRequestingUserFromResults()
    {
        var requestingUser = User.Create("me@test.com", "Me", "hash");
        var otherUser = User.Create("other@test.com", "Other", "hash");
        _userRepo.SearchAsync("test", default).Returns([requestingUser, otherUser]);

        var result = (await _handler.Handle(
            new SearchUsersQuery(requestingUser.Id, "test"), default)).ToList();

        result.Should().HaveCount(1);
        result[0].Email.Should().Be("other@test.com");
    }

    [Fact]
    public async Task Handle_WithQueryTooShort_ReturnsEmptyWithoutCallingRepo()
    {
        var result = await _handler.Handle(
            new SearchUsersQuery(Guid.NewGuid(), "j"), default);

        result.Should().BeEmpty();
        await _userRepo.DidNotReceive().SearchAsync(Arg.Any<string>(), default);
    }

    [Fact]
    public async Task Handle_WithEmptyQuery_ReturnsEmptyWithoutCallingRepo()
    {
        var result = await _handler.Handle(
            new SearchUsersQuery(Guid.NewGuid(), ""), default);

        result.Should().BeEmpty();
        await _userRepo.DidNotReceive().SearchAsync(Arg.Any<string>(), default);
    }
}
