using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WorkCale.Api.IntegrationTests.Helpers;
using WorkCale.Application.DTOs;
using Xunit;

namespace WorkCale.Api.IntegrationTests.Auth;

public class AuthEndpointTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly TestWebAppFactory _factory = factory;

    private static string UniqueEmail() => $"auth-{Guid.NewGuid():N}@test.com";

    [Fact]
    public async Task Register_WithValidData_ReturnsAuthResult()
    {
        var email = UniqueEmail();
        var invite = await AuthHelper.IssueInviteAsync(_factory);
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "Test User", "Password123!", invite));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var email = UniqueEmail();
        var invite1 = await AuthHelper.IssueInviteAsync(_factory);
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "User", "Password123!", invite1));

        // Second attempt uses a fresh invite so the failure is on the duplicate email, not a spent code.
        var invite2 = await AuthHelper.IssueInviteAsync(_factory);
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "User2", "Password123!", invite2));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithoutInviteCode_ReturnsBadRequest()
    {
        var email = UniqueEmail();
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "NoInvite", "Password123!", ""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithUnknownInviteCode_Rejected()
    {
        var email = UniqueEmail();
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "BadCode", "Password123!", "WC-NOPE-0000"));

        // Unknown invite surfaces as a 400/409 via the exception handler; either is fine —
        // the guarantee is that it's NOT a 200 / account creation.
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAuthResult()
    {
        var email = UniqueEmail();
        var invite = await AuthHelper.IssueInviteAsync(_factory);
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "Login User", "Password123!", invite));

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, "Password123!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        result!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = UniqueEmail();
        var invite = await AuthHelper.IssueInviteAsync(_factory);
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "User", "Password123!", invite));

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, "WrongPassword!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsCurrentUser()
    {
        var email = UniqueEmail();
        var auth = await AuthHelper.RegisterAndLoginAsync(_client, _factory, email);
        _client.SetBearerToken(auth.AccessToken);

        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user!.Email.Should().Be(email);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        var auth = await AuthHelper.RegisterAndLoginAsync(_client, _factory);

        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequest(auth.RefreshToken));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(auth.RefreshToken);
    }
}
