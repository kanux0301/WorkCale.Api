using WorkCale.Domain.Entities;

namespace WorkCale.Application.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
