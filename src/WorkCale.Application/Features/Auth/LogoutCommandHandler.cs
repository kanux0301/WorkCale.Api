using WorkCale.Application.Services;
using MediatR;

namespace WorkCale.Application.Features.Auth;

public class LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken ct)
    {
        var token = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, ct);
        if (token is not null)
            await refreshTokenRepository.DeleteAsync(token, ct);
    }
}
