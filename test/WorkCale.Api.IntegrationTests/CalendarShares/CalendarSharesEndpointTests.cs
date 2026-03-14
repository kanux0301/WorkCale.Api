using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WorkCale.Api.IntegrationTests.Helpers;
using WorkCale.Application.DTOs;
using Xunit;

namespace WorkCale.Api.IntegrationTests.CalendarShares;

public class CalendarSharesEndpointTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    private async Task<(HttpClient client, AuthResult auth)> SetupAsync()
    {
        var client = factory.CreateClient();
        var auth = await AuthHelper.RegisterAndLoginAsync(client);
        client.SetBearerToken(auth.AccessToken);
        return (client, auth);
    }

    [Fact]
    public async Task GrantShare_ThenViewerCanReadSharedCalendar()
    {
        var (clientA, authA) = await SetupAsync();
        var (clientB, authB) = await SetupAsync();

        var grantResp = await clientA.PostAsJsonAsync("/api/calendar-shares",
            new GrantShareRequest(authB.User.Id));
        grantResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var calResp = await clientB.GetAsync(
            $"/api/calendar-shares/from/{authA.User.Id}?year=2026&month=3");
        calResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RevokeShare_ThenViewerGets403()
    {
        var (clientA, authA) = await SetupAsync();
        var (clientB, authB) = await SetupAsync();

        var grantResp = await clientA.PostAsJsonAsync("/api/calendar-shares",
            new GrantShareRequest(authB.User.Id));
        var share = await grantResp.Content.ReadFromJsonAsync<CalendarShareDto>();

        await clientA.DeleteAsync($"/api/calendar-shares/{share!.Id}");

        var calResp = await clientB.GetAsync(
            $"/api/calendar-shares/from/{authA.User.Id}?year=2026&month=3");
        calResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ViewSharedCalendar_WithoutShare_Returns401()
    {
        var (clientA, authA) = await SetupAsync();
        var (clientB, authB) = await SetupAsync();

        var response = await clientB.GetAsync(
            $"/api/calendar-shares/from/{authA.User.Id}?year=2026&month=3");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
