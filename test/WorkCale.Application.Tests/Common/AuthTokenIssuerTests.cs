using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Common;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.Common;

public class AuthTokenIssuerTests
{
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();

    [Fact]
    public async Task IssueAsync_ReturnsAuthResultWithGeneratedTokens()
    {
        var user = User.Create("a@b.com", "Alice", "hash");
        _jwt.GenerateAccessToken(user).Returns("access-token");
        _jwt.GenerateRefreshToken().Returns("refresh-token");

        var result = await AuthTokenIssuer.IssueAsync(_jwt, _refreshTokens, user, CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.User.Email.Should().Be("a@b.com");
        result.User.DisplayName.Should().Be("Alice");
    }

    [Fact]
    public async Task IssueAsync_PersistsRefreshTokenForUser()
    {
        var user = User.Create("a@b.com", "Alice", "hash");
        _jwt.GenerateRefreshToken().Returns("refresh-token");

        await AuthTokenIssuer.IssueAsync(_jwt, _refreshTokens, user, CancellationToken.None);

        await _refreshTokens.Received(1).AddAsync(
            Arg.Is<RefreshToken>(t => t.UserId == user.Id && t.Token == "refresh-token"),
            Arg.Any<CancellationToken>());
    }
}
