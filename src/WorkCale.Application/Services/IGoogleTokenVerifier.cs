namespace WorkCale.Application.Services;

public record GoogleUserInfo(string GoogleId, string Email, string Name, string? Picture);

public interface IGoogleTokenVerifier
{
    Task<GoogleUserInfo> VerifyAsync(string idToken, CancellationToken ct = default);
}
