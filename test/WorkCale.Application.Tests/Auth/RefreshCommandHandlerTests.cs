using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.Auth;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.Auth;

public class RefreshCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshRepo = Substitute.For<IRefreshTokenRepository>();
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly RefreshCommandHandler _handler;

    public RefreshCommandHandlerTests()
    {
        _handler = new RefreshCommandHandler(_refreshRepo, _userRepo, _jwt);
        _jwt.GenerateAccessToken(Arg.Any<User>()).Returns("new_access");
        _jwt.GenerateRefreshToken().Returns("new_refresh");
        _refreshRepo.DeleteAsync(Arg.Any<RefreshToken>(), default).Returns(Task.CompletedTask);
        _refreshRepo.AddAsync(Arg.Any<RefreshToken>(), default).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_WithValidToken_RotatesRefreshToken()
    {
        var user = User.Create("u@test.com", "User", "hash");
        var token = RefreshToken.Create(user.Id, "old_token");
        _refreshRepo.GetByTokenAsync("old_token", default).Returns(token);
        _userRepo.GetByIdAsync(user.Id, default).Returns(user);

        var result = await _handler.Handle(new RefreshCommand("old_token"), default);

        result.AccessToken.Should().Be("new_access");
        result.RefreshToken.Should().Be("new_refresh");
        await _refreshRepo.Received(1).DeleteAsync(token, default);
        await _refreshRepo.Received(1).AddAsync(Arg.Any<RefreshToken>(), default);
    }

    [Fact]
    public async Task Handle_WithUnknownToken_ThrowsUnauthorized()
    {
        _refreshRepo.GetByTokenAsync("bad_token", default).Returns((RefreshToken?)null);

        var act = () => _handler.Handle(new RefreshCommand("bad_token"), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid or expired*");
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ThrowsUnauthorized()
    {
        var userId = Guid.NewGuid();
        // Create a real token then back-date ExpiresAt via reflection
        var token = RefreshToken.Create(userId, "expired");
        typeof(RefreshToken).GetProperty("ExpiresAt")!
            .SetValue(token, DateTime.UtcNow.AddDays(-1));
        _refreshRepo.GetByTokenAsync("expired", default).Returns(token);

        var act = () => _handler.Handle(new RefreshCommand("expired"), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid or expired*");
    }
}
