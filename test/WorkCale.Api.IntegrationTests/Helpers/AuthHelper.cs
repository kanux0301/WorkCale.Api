using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WorkCale.Application.DTOs;
using WorkCale.Application.Services;
using WorkCale.Domain.Entities;

namespace WorkCale.Api.IntegrationTests.Helpers;

public static class AuthHelper
{
    /// <summary>
    /// Seeds a redeemable invite code directly in the test DB so registration
    /// tests don't need to stand up an admin first. Returns the code string.
    /// </summary>
    public static async Task<string> IssueInviteAsync(TestWebAppFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IInviteCodeRepository>();
        // Short unique code, upper-cased to match the repository's lookup normalisation.
        var code = $"WC-TST-{Guid.NewGuid():N}"[..13].ToUpperInvariant();
        var invite = InviteCode.Issue(Guid.NewGuid(), code, null, "integration-test");
        await repo.AddAsync(invite);
        return code;
    }

    public static async Task<AuthResult> RegisterAndLoginAsync(
        HttpClient client,
        TestWebAppFactory factory,
        string? email = null,
        string displayName = "Test User",
        string password = "TestPassword123!")
    {
        email ??= $"user-{Guid.NewGuid():N}@workcale.com";
        var inviteCode = await IssueInviteAsync(factory);
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, displayName, password, inviteCode));
        registerResponse.EnsureSuccessStatusCode();
        return (await registerResponse.Content.ReadFromJsonAsync<AuthResult>())!;
    }

    public static void SetBearerToken(this HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    }
}
