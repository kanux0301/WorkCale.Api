using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;
using MediatR;

namespace WorkCale.Application.Features.Auth;

public class GoogleLoginCommandHandler(
    IUserRepository userRepository,
    IShiftCategoryRepository categoryRepository,
    IGoogleTokenVerifier googleTokenVerifier,
    IJwtService jwtService,
    IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<GoogleLoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(GoogleLoginCommand request, CancellationToken ct)
    {
        var googleUser = await googleTokenVerifier.VerifyAsync(request.IdToken, ct);

        // Try find by GoogleId first, then by email
        var user = await userRepository.GetByGoogleIdAsync(googleUser.GoogleId, ct)
                   ?? await userRepository.GetByEmailAsync(googleUser.Email, ct);

        if (user is null)
        {
            // New user — create and seed default categories
            user = User.CreateWithGoogle(googleUser.Email, googleUser.Name, googleUser.GoogleId, googleUser.Picture);
            await userRepository.AddAsync(user, ct);

            await categoryRepository.AddAsync(ShiftCategory.Create(user.Id, "Day Shift", "#F59E0B"), ct);
            await categoryRepository.AddAsync(ShiftCategory.Create(user.Id, "Night Shift", "#6366F1"), ct);
        }
        else if (user.GoogleId is null)
        {
            // Existing email/password account — link Google
            user.LinkGoogle(googleUser.GoogleId, googleUser.Picture);
            await userRepository.UpdateAsync(user, ct);
        }

        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshTokenValue = jwtService.GenerateRefreshToken();
        var refreshToken = RefreshToken.Create(user.Id, refreshTokenValue);
        await refreshTokenRepository.AddAsync(refreshToken, ct);

        var userDto = new UserDto(user.Id, user.Email, user.DisplayName, user.AvatarUrl);
        return new AuthResult(accessToken, refreshTokenValue, userDto);
    }
}
