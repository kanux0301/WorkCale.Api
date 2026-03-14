using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using MediatR;

namespace WorkCale.Application.Features.Auth;

public class RefreshCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IJwtService jwtService)
    : IRequestHandler<RefreshCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RefreshCommand request, CancellationToken ct)
    {
        var token = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, ct);

        if (token is null || token.IsExpired)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = await userRepository.GetByIdAsync(token.UserId, ct)
                   ?? throw new UnauthorizedAccessException("User not found.");

        // Rotate: delete old, issue new
        await refreshTokenRepository.DeleteAsync(token, ct);

        var newRefreshTokenValue = jwtService.GenerateRefreshToken();
        var newRefreshToken = RefreshToken.Create(user.Id, newRefreshTokenValue);
        await refreshTokenRepository.AddAsync(newRefreshToken, ct);

        var accessToken = jwtService.GenerateAccessToken(user);
        var userDto = new UserDto(user.Id, user.Email, user.DisplayName, user.AvatarUrl);
        return new AuthResult(accessToken, newRefreshTokenValue, userDto);
    }
}
