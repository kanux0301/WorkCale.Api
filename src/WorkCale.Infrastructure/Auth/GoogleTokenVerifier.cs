using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using WorkCale.Application.Services;

namespace WorkCale.Infrastructure.Auth;

public class GoogleTokenVerifier(IConfiguration configuration) : IGoogleTokenVerifier
{
    public async Task<GoogleUserInfo> VerifyAsync(string idToken, CancellationToken ct = default)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [configuration["Google:ClientId"]!]
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedAccessException("Invalid Google ID token.", ex);
        }

        return new GoogleUserInfo(
            GoogleId: payload.Subject,
            Email: payload.Email,
            Name: payload.Name ?? payload.Email,
            Picture: payload.Picture);
    }
}
