using MediatR;
using WorkCale.Application.Common;
using WorkCale.Application.DTOs;
using WorkCale.Application.Services;

namespace WorkCale.Application.Features.Auth;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, ct);

        if (user is null || user.PasswordHash is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return await AuthTokenIssuer.IssueAsync(jwtService, refreshTokenRepository, user, ct);
    }
}
