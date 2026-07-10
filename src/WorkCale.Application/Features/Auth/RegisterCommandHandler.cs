using MediatR;
using WorkCale.Application.Common;
using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Application.Features.Auth;

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IJobRepository jobRepository,
    IShiftCategoryRepository categoryRepository,
    IInviteCodeRepository inviteCodeRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<RegisterCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        var invite = await inviteCodeRepository.GetByCodeAsync(request.InviteCode, ct);
        if (invite is null || !invite.IsRedeemable(DateTime.UtcNow))
            throw new InvalidOperationException("Invalid or already-used invite code.");

        if (await userRepository.GetByEmailAsync(request.Email, ct) is not null)
            throw new InvalidOperationException("An account with this email already exists.");

        var user = User.Create(request.Email, request.DisplayName, passwordHasher.Hash(request.Password));
        await userRepository.AddAsync(user, ct);

        invite.Consume(user.Id, DateTime.UtcNow);
        await inviteCodeRepository.UpdateAsync(invite, ct);

        var defaultJob = await DefaultJob.SeedAsync(jobRepository, user.Id, ct);
        await DefaultCategories.SeedAsync(categoryRepository, user.Id, defaultJob.Id, ct);

        return await AuthTokenIssuer.IssueAsync(jwtService, refreshTokenRepository, user, ct);
    }
}
