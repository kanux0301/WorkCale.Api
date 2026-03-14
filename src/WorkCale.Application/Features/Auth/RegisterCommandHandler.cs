using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using MediatR;

namespace WorkCale.Application.Features.Auth;

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IShiftCategoryRepository categoryRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<RegisterCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            throw new InvalidOperationException("An account with this email already exists.");

        var hash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, request.DisplayName, hash);
        await userRepository.AddAsync(user, ct);

        // Seed 2 default categories
        await categoryRepository.AddAsync(ShiftCategory.Create(user.Id, "Day Shift", "#F59E0B"), ct);
        await categoryRepository.AddAsync(ShiftCategory.Create(user.Id, "Night Shift", "#6366F1"), ct);

        return await IssueTokens(user, ct);
    }

    private async Task<AuthResult> IssueTokens(User user, CancellationToken ct)
    {
        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshTokenValue = jwtService.GenerateRefreshToken();
        var refreshToken = RefreshToken.Create(user.Id, refreshTokenValue);
        await refreshTokenRepository.AddAsync(refreshToken, ct);

        var userDto = new UserDto(user.Id, user.Email, user.DisplayName, user.AvatarUrl);
        return new AuthResult(accessToken, refreshTokenValue, userDto);
    }
}
