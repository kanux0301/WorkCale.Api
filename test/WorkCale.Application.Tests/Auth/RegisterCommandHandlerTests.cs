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
    private readonly IJobRepository _jobRepo = Substitute.For<IJobRepository>();
    private readonly IShiftCategoryRepository _categoryRepo = Substitute.For<IShiftCategoryRepository>();
    private readonly IInviteCodeRepository _inviteRepo = Substitute.For<IInviteCodeRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly IRefreshTokenRepository _refreshRepo = Substitute.For<IRefreshTokenRepository>();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_userRepo, _jobRepo, _categoryRepo, _inviteRepo, _hasher, _jwt, _refreshRepo);
        _jobRepo.AddAsync(Arg.Any<Job>(), default).Returns(Task.CompletedTask);
        _hasher.Hash(Arg.Any<string>()).Returns("hashed_pw");
        _jwt.GenerateAccessToken(Arg.Any<User>()).Returns("access_token");
        _jwt.GenerateRefreshToken().Returns("refresh_token");
        _refreshRepo.AddAsync(Arg.Any<RefreshToken>(), default).Returns(Task.CompletedTask);
        _userRepo.AddAsync(Arg.Any<User>(), default).Returns(Task.CompletedTask);
        _categoryRepo.AddAsync(Arg.Any<ShiftCategory>(), default).Returns(Task.CompletedTask);
        // Default: any invite lookup returns a fresh redeemable code so happy-path tests stay terse.
        _inviteRepo.GetByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => InviteCode.Issue(Guid.NewGuid(), "WC-TEST-0000", null, null));
    }

    [Fact]
    public async Task Handle_WithNewEmail_ReturnsAuthResultWithTokens()
    {
        _userRepo.GetByEmailAsync("new@test.com", default).Returns((User?)null);

        var result = await _handler.Handle(new RegisterCommand("new@test.com", "Jane", "Password123!", "WC-TEST-0000"), default);

        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.User.Email.Should().Be("new@test.com");
        result.User.DisplayName.Should().Be("Jane");
    }

    [Fact]
    public async Task Handle_WithNewEmail_SeedsTwoDefaultCategories()
    {
        _userRepo.GetByEmailAsync("seed@test.com", default).Returns((User?)null);

        await _handler.Handle(new RegisterCommand("seed@test.com", "User", "pw", "WC-TEST-0000"), default);

        await _categoryRepo.Received(2).AddAsync(Arg.Any<ShiftCategory>(), default);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ThrowsInvalidOperationException()
    {
        var existing = User.Create("exists@test.com", "Existing", "hash");
        _userRepo.GetByEmailAsync("exists@test.com", default).Returns(existing);

        var act = () => _handler.Handle(new RegisterCommand("exists@test.com", "Jane", "pw", "WC-TEST-0000"), default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email already exists*");
    }

    [Fact]
    public async Task Handle_WithUnknownInviteCode_ThrowsInvalidOperationException()
    {
        _inviteRepo.GetByCodeAsync("WC-BAD-CODE", Arg.Any<CancellationToken>())
            .Returns((InviteCode?)null);

        var act = () => _handler.Handle(new RegisterCommand("x@test.com", "X", "pw", "WC-BAD-CODE"), default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*invite code*");
        await _userRepo.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAlreadyConsumedInvite_ThrowsInvalidOperationException()
    {
        var consumed = InviteCode.Issue(Guid.NewGuid(), "WC-USED-0000", null, null);
        consumed.Consume(Guid.NewGuid(), DateTime.UtcNow);
        _inviteRepo.GetByCodeAsync("WC-USED-0000", Arg.Any<CancellationToken>()).Returns(consumed);

        var act = () => _handler.Handle(new RegisterCommand("x@test.com", "X", "pw", "WC-USED-0000"), default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*invite code*");
    }
}
