using FluentAssertions;
using NSubstitute;
using WorkCale.Application.Features.Auth;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Application.Tests.Auth;

public class RegisterCommandHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IShiftCategoryRepository _categoryRepo = Substitute.For<IShiftCategoryRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly IRefreshTokenRepository _refreshRepo = Substitute.For<IRefreshTokenRepository>();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_userRepo, _categoryRepo, _hasher, _jwt, _refreshRepo);
        _hasher.Hash(Arg.Any<string>()).Returns("hashed_pw");
        _jwt.GenerateAccessToken(Arg.Any<User>()).Returns("access_token");
        _jwt.GenerateRefreshToken().Returns("refresh_token");
        _refreshRepo.AddAsync(Arg.Any<RefreshToken>(), default).Returns(Task.CompletedTask);
        _userRepo.AddAsync(Arg.Any<User>(), default).Returns(Task.CompletedTask);
        _categoryRepo.AddAsync(Arg.Any<ShiftCategory>(), default).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_WithNewEmail_ReturnsAuthResultWithTokens()
    {
        _userRepo.GetByEmailAsync("new@test.com", default).Returns((User?)null);

        var result = await _handler.Handle(new RegisterCommand("new@test.com", "Jane", "Password123!"), default);

        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.User.Email.Should().Be("new@test.com");
        result.User.DisplayName.Should().Be("Jane");
    }

    [Fact]
    public async Task Handle_WithNewEmail_SeedsTwoDefaultCategories()
    {
        _userRepo.GetByEmailAsync("seed@test.com", default).Returns((User?)null);

        await _handler.Handle(new RegisterCommand("seed@test.com", "User", "pw"), default);

        await _categoryRepo.Received(2).AddAsync(Arg.Any<ShiftCategory>(), default);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ThrowsInvalidOperationException()
    {
        var existing = User.Create("exists@test.com", "Existing", "hash");
        _userRepo.GetByEmailAsync("exists@test.com", default).Returns(existing);

        var act = () => _handler.Handle(new RegisterCommand("exists@test.com", "Jane", "pw"), default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email already exists*");
    }
}
