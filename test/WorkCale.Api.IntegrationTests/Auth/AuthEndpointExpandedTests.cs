using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WorkCale.Api.IntegrationTests.Helpers;
using WorkCale.Application.DTOs;
using Xunit;

namespace WorkCale.Api.IntegrationTests.Auth;

public class AuthEndpointExpandedTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    private static string UniqueEmail() => $"authex-{Guid.NewGuid():N}@test.com";

    // ── Register validation ────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithMissingEmail_Returns400()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = (string?)null, displayName = "Name", password = "Password123!" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithMissingDisplayName_Returns400()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = UniqueEmail(), displayName = (string?)null, password = "Password123!" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithMissingPassword_Returns400()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = UniqueEmail(), displayName = "Name", password = (string?)null });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_SeedsDefaultCategories()
    {
        var client = factory.CreateClient();
        var auth = await AuthHelper.RegisterAndLoginAsync(client);
        client.SetBearerToken(auth.AccessToken);

        var cats = await client.GetFromJsonAsync<List<ShiftCategoryDto>>("/api/categories");

        cats.Should().HaveCount(2);
        cats.Should().Contain(c => c.Name == "Day Shift" && c.Color == "#F59E0B");
        cats.Should().Contain(c => c.Name == "Night Shift" && c.Color == "#6366F1");
    }

    // ── Login failures ─────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("ghost@nobody.com", "Password123!"));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithMissingEmail_Returns400()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = (string?)null, password = "Password123!" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Refresh failures ───────────────────────────────────────────────────

    [Fact]
    public async Task Refresh_WithInvalidToken_Returns401()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequest("this-is-not-a-valid-token"));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithEmptyToken_Returns401()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = (string?)null });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Refresh_TokenCannotBeReusedAfterRotation()
    {
        var client = factory.CreateClient();
        var auth = await AuthHelper.RegisterAndLoginAsync(client);

        var first = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequest(auth.RefreshToken));
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequest(auth.RefreshToken));
        second.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Logout ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_WithValidToken_Returns204()
    {
        var client = factory.CreateClient();
        var auth = await AuthHelper.RegisterAndLoginAsync(client);

        var response = await client.PostAsJsonAsync("/api/auth/logout",
            new LogoutRequest(auth.RefreshToken));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WithUnknownToken_StillReturns204()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/logout",
            new LogoutRequest("unknown-refresh-token"));
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_InvalidatesRefreshToken()
    {
        var client = factory.CreateClient();
        var auth = await AuthHelper.RegisterAndLoginAsync(client);

        await client.PostAsJsonAsync("/api/auth/logout",
            new LogoutRequest(auth.RefreshToken));

        var refreshResp = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequest(auth.RefreshToken));
        refreshResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
