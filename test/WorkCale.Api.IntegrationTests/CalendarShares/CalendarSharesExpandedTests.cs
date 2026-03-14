using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WorkCale.Api.IntegrationTests.Helpers;
using WorkCale.Application.DTOs;
using Xunit;

namespace WorkCale.Api.IntegrationTests.CalendarShares;

public class CalendarSharesExpandedTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    private async Task<(HttpClient client, AuthResult auth)> SetupAsync()
    {
        var client = factory.CreateClient();
        var auth = await AuthHelper.RegisterAndLoginAsync(client);
        client.SetBearerToken(auth.AccessToken);
        return (client, auth);
    }

    [Fact]
    public async Task GetMine_WithNoShares_ReturnsEmptyLists()
    {
        var (client, _) = await SetupAsync();

        var result = await client.GetFromJsonAsync<MySharesDto>("/api/calendar-shares/mine");

        result!.GrantedByMe.Should().BeEmpty();
        result.GrantedToMe.Should().BeEmpty();
    }

    [Fact]
    public async Task GrantShare_ToSelf_Returns400()
    {
        var (client, auth) = await SetupAsync();

        var response = await client.PostAsJsonAsync("/api/calendar-shares",
            new GrantShareRequest(auth.User.Id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GrantShare_ToNonExistentUser_Returns404()
    {
        var (client, _) = await SetupAsync();

        var response = await client.PostAsJsonAsync("/api/calendar-shares",
            new GrantShareRequest(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GrantShare_Duplicate_Returns409()
    {
        var (clientA, authA) = await SetupAsync();
        var (_, authB) = await SetupAsync();

        await clientA.PostAsJsonAsync("/api/calendar-shares",
            new GrantShareRequest(authB.User.Id));

        var response = await clientA.PostAsJsonAsync("/api/calendar-shares",
            new GrantShareRequest(authB.User.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GrantShare_AppearsInGrantedByMeList()
    {
        var (clientA, authA) = await SetupAsync();
        var (_, authB) = await SetupAsync();

        await clientA.PostAsJsonAsync("/api/calendar-shares",
            new GrantShareRequest(authB.User.Id));

        var result = await clientA.GetFromJsonAsync<MySharesDto>("/api/calendar-shares/mine");

        result!.GrantedByMe.Should().ContainSingle();
        result.GrantedByMe.First().User.Id.Should().Be(authB.User.Id);
    }

    [Fact]
    public async Task RevokeShare_NotFound_Returns404()
    {
        var (client, _) = await SetupAsync();

        var response = await client.DeleteAsync($"/api/calendar-shares/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ViewSharedCalendar_WithNoShare_Returns401()
    {
        var (clientA, authA) = await SetupAsync();
        var (clientB, authB) = await SetupAsync();

        var response = await clientB.GetAsync(
            $"/api/calendar-shares/from/{authA.User.Id}?year=2026&month=3");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ViewSharedCalendar_IncludesOwnerShifts()
    {
        var (clientA, authA) = await SetupAsync();
        var (clientB, authB) = await SetupAsync();

        await clientA.PostAsJsonAsync("/api/calendar-shares",
            new GrantShareRequest(authB.User.Id));

        var cats = await clientA.GetFromJsonAsync<List<ShiftCategoryDto>>("/api/categories");
        await clientA.PostAsJsonAsync("/api/shifts",
            new CreateShiftRequest(new DateOnly(2026, 3, 15), "08:00", "16:00",
                cats![0].Id, null, null));

        var result = await clientB.GetFromJsonAsync<SharedCalendarDto>(
            $"/api/calendar-shares/from/{authA.User.Id}?year=2026&month=3");

        result!.Shifts.Should().ContainSingle();
    }
}
