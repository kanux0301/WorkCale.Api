using MediatR;
using WorkCale.Application.Common;
using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Application.Features.Auth;

public class GoogleLoginCommandHandler(
    IUserRepository userRepository,
    IShiftCategoryRepository categoryRepository,
    IInviteCodeRepository inviteCodeRepository,
    IGoogleTokenVerifier googleTokenVerifier,
    IJwtService jwtService,
    IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<GoogleLoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(GoogleLoginCommand request, CancellationToken ct)
    {
        var googleUser = await googleTokenVerifier.VerifyAsync(request.IdToken, ct);

        var user = await userRepository.GetByGoogleIdAsync(googleUser.GoogleId, ct)
                   ?? await userRepository.GetByEmailAsync(googleUser.Email, ct);

        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(request.InviteCode))
                throw new InvalidOperationException("Invite code is required for new accounts.");

            var invite = await inviteCodeRepository.GetByCodeAsync(request.InviteCode, ct);
            if (invite is null || !invite.IsRedeemable(DateTime.UtcNow))
                throw new InvalidOperationException("Invalid or already-used invite code.");

            user = User.CreateWithGoogle(googleUser.Email, googleUser.Name, googleUser.GoogleId, googleUser.Picture);
            await userRepository.AddAsync(user, ct);

            invite.Consume(user.Id, DateTime.UtcNow);
            await inviteCodeRepository.UpdateAsync(invite, ct);

            await DefaultCategories.SeedAsync(categoryRepository, user.Id, ct);
        }
        else if (user.GoogleId is null)
        {
            user.LinkGoogle(googleUser.GoogleId, googleUser.Picture);
            await userRepository.UpdateAsync(user, ct);
        }

        return await AuthTokenIssuer.IssueAsync(jwtService, refreshTokenRepository, user, ct);
    }
}
