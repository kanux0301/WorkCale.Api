using System.Net.Http.Json;
using WorkCale.Application.DTOs;

namespace WorkCale.Api.IntegrationTests.Helpers;

public static class AuthHelper
{
    public static async Task<AuthResult> RegisterAndLoginAsync(
        HttpClient client,
        string? email = null,
        string displayName = "Test User",
        string password = "TestPassword123!")
    {
        email ??= $"user-{Guid.NewGuid():N}@workcale.com";
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, displayName, password));
        registerResponse.EnsureSuccessStatusCode();
        return (await registerResponse.Content.ReadFromJsonAsync<AuthResult>())!;
    }

    public static void SetBearerToken(this HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    }
}
