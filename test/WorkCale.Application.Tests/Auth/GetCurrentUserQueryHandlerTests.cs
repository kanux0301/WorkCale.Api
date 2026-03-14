using Xunit;
using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.Auth;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Application.Tests.Auth;

public class GetCurrentUserQueryHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly GetCurrentUserQueryHandler _sut;

    public GetCurrentUserQueryHandlerTests()
    {
        _sut = new GetCurrentUserQueryHandler(_userRepo);
    }

    [Fact]
    public async Task Handle_ReturnsUserDto_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var user = User.Create("alice@example.com", "Alice", "hash");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _sut.Handle(new GetCurrentUserQuery(userId), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be("alice@example.com");
        result.DisplayName.Should().Be("Alice");
    }

    [Fact]
    public async Task Handle_ThrowsKeyNotFoundException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = async () => await _sut.Handle(new GetCurrentUserQuery(userId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_ReturnsAvatarUrl_WhenGoogleUser()
    {
        var user = User.CreateWithGoogle("g@x.com", "Google User", "gid", "https://avatar.url");

        _userRepo.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _sut.Handle(new GetCurrentUserQuery(user.Id), CancellationToken.None);

        result.AvatarUrl.Should().Be("https://avatar.url");
    }
}
