using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Application.Common;

public static class AuthTokenIssuer
{
    public static async Task<AuthResult> IssueAsync(
        IJwtService jwt,
        IRefreshTokenRepository refreshTokens,
        User user,
        CancellationToken ct)
    {
        var refresh = jwt.GenerateRefreshToken();
        await refreshTokens.AddAsync(RefreshToken.Create(user.Id, refresh), ct);
        return new AuthResult(jwt.GenerateAccessToken(user), refresh, user.ToDto());
    }
}
