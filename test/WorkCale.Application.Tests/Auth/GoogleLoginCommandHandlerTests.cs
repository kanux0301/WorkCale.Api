using Xunit;
using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.Auth;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Application.Tests.Auth;

public class GoogleLoginCommandHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IShiftCategoryRepository _catRepo = Substitute.For<IShiftCategoryRepository>();
    private readonly IGoogleTokenVerifier _google = Substitute.For<IGoogleTokenVerifier>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly IRefreshTokenRepository _tokenRepo = Substitute.For<IRefreshTokenRepository>();
    private readonly GoogleLoginCommandHandler _sut;

    private static readonly GoogleUserInfo GoogleUser = new("gid123", "alice@google.com", "Alice", "https://avatar");

    public GoogleLoginCommandHandlerTests()
    {
        _sut = new GoogleLoginCommandHandler(_userRepo, _catRepo, _google, _jwt, _tokenRepo);
        _jwt.GenerateAccessToken(Arg.Any<User>()).Returns("access_token");
        _jwt.GenerateRefreshToken().Returns("refresh_token");
    }

    [Fact]
    public async Task Handle_CreatesNewUser_WhenNotFound()
    {
        _google.VerifyAsync("valid_id_token", Arg.Any<CancellationToken>())
            .Returns(GoogleUser);
        _userRepo.GetByGoogleIdAsync("gid123", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepo.GetByEmailAsync("alice@google.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _sut.Handle(new GoogleLoginCommand("valid_id_token"), CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.User.Email.Should().Be("alice@google.com");

        await _userRepo.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _catRepo.Received(2).AddAsync(Arg.Any<ShiftCategory>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsExistingUser_WhenFoundByGoogleId()
    {
        var existingUser = User.CreateWithGoogle("alice@google.com", "Alice", "gid123", null);

        _google.VerifyAsync("id_token", Arg.Any<CancellationToken>())
            .Returns(GoogleUser);
        _userRepo.GetByGoogleIdAsync("gid123", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var result = await _sut.Handle(new GoogleLoginCommand("id_token"), CancellationToken.None);

        result.User.Email.Should().Be("alice@google.com");
        await _userRepo.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _catRepo.DidNotReceive().AddAsync(Arg.Any<ShiftCategory>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LinksGoogle_WhenExistingEmailUser()
    {
        var existingUser = User.Create("alice@google.com", "Alice", "password_hash");

        _google.VerifyAsync("id_token", Arg.Any<CancellationToken>())
            .Returns(GoogleUser);
        _userRepo.GetByGoogleIdAsync("gid123", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepo.GetByEmailAsync("alice@google.com", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        await _sut.Handle(new GoogleLoginCommand("id_token"), CancellationToken.None);

        existingUser.GoogleId.Should().Be("gid123");
        await _userRepo.Received(1).UpdateAsync(existingUser, Arg.Any<CancellationToken>());
        await _catRepo.DidNotReceive().AddAsync(Arg.Any<ShiftCategory>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_WhenInvalidGoogleToken()
    {
        _google.VerifyAsync("bad_token", Arg.Any<CancellationToken>())
            .Returns(Task.FromException<GoogleUserInfo>(new UnauthorizedAccessException("Invalid Google ID token.")));

        var act = async () => await _sut.Handle(new GoogleLoginCommand("bad_token"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_AlwaysCreatesRefreshToken()
    {
        var existingUser = User.CreateWithGoogle("alice@google.com", "Alice", "gid123", null);

        _google.VerifyAsync("id_token", Arg.Any<CancellationToken>())
            .Returns(GoogleUser);
        _userRepo.GetByGoogleIdAsync("gid123", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        await _sut.Handle(new GoogleLoginCommand("id_token"), CancellationToken.None);

        await _tokenRepo.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }
}
