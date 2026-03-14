using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.Auth;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly IRefreshTokenRepository _refreshRepo = Substitute.For<IRefreshTokenRepository>();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_userRepo, _hasher, _jwt, _refreshRepo);
        _jwt.GenerateAccessToken(Arg.Any<User>()).Returns("access_token");
        _jwt.GenerateRefreshToken().Returns("refresh_token");
        _refreshRepo.AddAsync(Arg.Any<RefreshToken>(), default).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsAuthResult()
    {
        var user = User.Create("jane@test.com", "Jane", "hashed");
        _userRepo.GetByEmailAsync("jane@test.com", default).Returns(user);
        _hasher.Verify("Password123!", "hashed").Returns(true);

        var result = await _handler.Handle(new LoginCommand("jane@test.com", "Password123!"), default);

        result.AccessToken.Should().Be("access_token");
        result.User.Email.Should().Be("jane@test.com");
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsUnauthorized()
    {
        var user = User.Create("jane@test.com", "Jane", "hashed");
        _userRepo.GetByEmailAsync("jane@test.com", default).Returns(user);
        _hasher.Verify("WrongPass", "hashed").Returns(false);

        var act = () => _handler.Handle(new LoginCommand("jane@test.com", "WrongPass"), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ThrowsUnauthorized()
    {
        _userRepo.GetByEmailAsync("ghost@test.com", default).Returns((User?)null);

        var act = () => _handler.Handle(new LoginCommand("ghost@test.com", "pw"), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ForGoogleUserWithNoPassword_ThrowsUnauthorized()
    {
        // Google users have null PasswordHash — cannot log in with email/password
        var googleUser = User.CreateWithGoogle("google@test.com", "Google User", "google-id-123", null);
        _userRepo.GetByEmailAsync("google@test.com", default).Returns(googleUser);

        var act = () => _handler.Handle(new LoginCommand("google@test.com", "anypassword"), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
