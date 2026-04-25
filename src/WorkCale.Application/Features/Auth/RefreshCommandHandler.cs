using MediatR;
using WorkCale.Application.Common;
using WorkCale.Application.DTOs;
using WorkCale.Application.Services;

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

        await refreshTokenRepository.DeleteAsync(token, ct);

        return await AuthTokenIssuer.IssueAsync(jwtService, refreshTokenRepository, user, ct);
    }
}
