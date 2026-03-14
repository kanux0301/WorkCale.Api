using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WorkCale.Api.IntegrationTests.Helpers;
using WorkCale.Application.DTOs;
using Xunit;

namespace WorkCale.Api.IntegrationTests.Shifts;

public class ShiftsEndpointExpandedTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    private async Task<(AuthResult auth, ShiftCategoryDto category, HttpClient client)> SetupAsync()
    {
        var client = factory.CreateClient();
        var auth = await AuthHelper.RegisterAndLoginAsync(client);
        client.SetBearerToken(auth.AccessToken);
        var cats = await client.GetFromJsonAsync<List<ShiftCategoryDto>>("/api/categories");
        return (auth, cats!.First(), client);
    }

    // ── GET /api/shifts ────────────────────────────────────────────────────

    [Fact]
    public async Task GetShifts_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/shifts?year=2026&month=3");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetShifts_WithNoShifts_ReturnsEmptyList()
    {
        var (_, _, client) = await SetupAsync();

        var response = await client.GetAsync("/api/shifts?year=2026&month=3");
        var shifts = await response.Content.ReadFromJsonAsync<List<ShiftDto>>();

        shifts.Should().BeEmpty();
    }

    // ── POST /api/shifts ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateShift_WithOtherUsersCategoryId_Returns401()
    {
        var (_, catA, _) = await SetupAsync();
        var (_, _, clientB) = await SetupAsync();

        var response = await clientB.PostAsJsonAsync("/api/shifts",
            new CreateShiftRequest(new DateOnly(2026, 3, 15), "09:00", "17:00", catA.Id, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateShift_WithOptionalFields_PersistsAll()
    {
        var (_, cat, client) = await SetupAsync();

        var created = await client.PostAsJsonAsync("/api/shifts",
            new CreateShiftRequest(new DateOnly(2026, 3, 15), "07:00", "15:00", cat.Id, "Ward B", "Night cover"));
        var shift = await created.Content.ReadFromJsonAsync<ShiftDto>();

        shift!.Location.Should().Be("Ward B");
        shift.Notes.Should().Be("Night cover");
    }

    // ── PUT /api/shifts/:id ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateShift_WithValidData_Returns200()
    {
        var (_, cat, client) = await SetupAsync();

        var created = await client.PostAsJsonAsync("/api/shifts",
            new CreateShiftRequest(new DateOnly(2026, 3, 15), "09:00", "17:00", cat.Id, null, null));
        var shift = await created.Content.ReadFromJsonAsync<ShiftDto>();

        var response = await client.PutAsJsonAsync($"/api/shifts/{shift!.Id}",
            new UpdateShiftRequest(new DateOnly(2026, 3, 15), "10:00", "18:00", cat.Id, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ShiftDto>();
        updated!.StartTime.Should().Be("10:00");
    }

    [Fact]
    public async Task UpdateShift_OwnedByOtherUser_Returns401()
    {
        var (_, catA, clientA) = await SetupAsync();
        var (_, _, clientB) = await SetupAsync();

        var createdResp = await clientA.PostAsJsonAsync("/api/shifts",
            new CreateShiftRequest(new DateOnly(2026, 3, 15), "09:00", "17:00", catA.Id, null, null));
        var shift = await createdResp.Content.ReadFromJsonAsync<ShiftDto>();

        var (_, catB, _) = (default(AuthResult), (await clientB.GetFromJsonAsync<List<ShiftCategoryDto>>("/api/categories"))!.First(), clientB);
        var response = await clientB.PutAsJsonAsync($"/api/shifts/{shift!.Id}",
            new UpdateShiftRequest(new DateOnly(2026, 3, 15), "10:00", "18:00", catB.Id, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── DELETE /api/shifts/:id ─────────────────────────────────────────────

    [Fact]
    public async Task DeleteShift_NotFound_Returns404()
    {
        var (_, _, client) = await SetupAsync();

        var response = await client.DeleteAsync($"/api/shifts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteShift_OwnedByOtherUser_Returns401()
    {
        var (_, catA, clientA) = await SetupAsync();
        var (_, _, clientB) = await SetupAsync();

        var createdResp = await clientA.PostAsJsonAsync("/api/shifts",
            new CreateShiftRequest(new DateOnly(2026, 3, 15), "09:00", "17:00", catA.Id, null, null));
        var shift = await createdResp.Content.ReadFromJsonAsync<ShiftDto>();

        var response = await clientB.DeleteAsync($"/api/shifts/{shift!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
