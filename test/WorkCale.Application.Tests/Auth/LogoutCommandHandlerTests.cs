using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.Auth;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.Auth;

public class LogoutCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshRepo = Substitute.For<IRefreshTokenRepository>();
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _handler = new LogoutCommandHandler(_refreshRepo);
        _refreshRepo.DeleteAsync(Arg.Any<RefreshToken>(), default).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_WithValidToken_DeletesToken()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "valid_token");
        _refreshRepo.GetByTokenAsync("valid_token", default).Returns(token);

        await _handler.Handle(new LogoutCommand("valid_token"), default);

        await _refreshRepo.Received(1).DeleteAsync(token, default);
    }

    [Fact]
    public async Task Handle_WithUnknownToken_DoesNotThrow()
    {
        _refreshRepo.GetByTokenAsync("unknown", default).Returns((RefreshToken?)null);

        // Should complete silently — logout is idempotent
        var act = () => _handler.Handle(new LogoutCommand("unknown"), default);

        await act.Should().NotThrowAsync();
        await _refreshRepo.DidNotReceive().DeleteAsync(Arg.Any<RefreshToken>(), default);
    }
}
