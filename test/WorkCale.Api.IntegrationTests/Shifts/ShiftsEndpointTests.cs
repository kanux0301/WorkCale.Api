using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WorkCale.Api.IntegrationTests.Helpers;
using WorkCale.Application.DTOs;
using Xunit;

namespace WorkCale.Api.IntegrationTests.Shifts;

public class ShiftsEndpointTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    private async Task<(HttpClient client, ShiftCategoryDto category)> SetupAsync()
    {
        var client = factory.CreateClient();
        var auth = await AuthHelper.RegisterAndLoginAsync(client);
        client.SetBearerToken(auth.AccessToken);
        var cats = await client.GetFromJsonAsync<List<ShiftCategoryDto>>("/api/categories");
        return (client, cats!.First());
    }

    [Fact]
    public async Task CreateShift_WithValidData_ReturnsShift()
    {
        var (client, cat) = await SetupAsync();

        var response = await client.PostAsJsonAsync("/api/shifts", new CreateShiftRequest(
            new DateOnly(2026, 3, 15), "07:00", "15:00", cat.Id, "Ward A", null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var shift = await response.Content.ReadFromJsonAsync<ShiftDto>();
        shift!.Category.Id.Should().Be(cat.Id);
        shift.StartTime.Should().Be("07:00");
    }

    [Fact]
    public async Task GetShifts_ReturnsOnlyCurrentMonthShifts()
    {
        var (client, cat) = await SetupAsync();

        await client.PostAsJsonAsync("/api/shifts", new CreateShiftRequest(
            new DateOnly(2026, 3, 10), "07:00", "15:00", cat.Id, null, null));
        await client.PostAsJsonAsync("/api/shifts", new CreateShiftRequest(
            new DateOnly(2026, 4, 5), "07:00", "15:00", cat.Id, null, null));

        var response = await client.GetAsync("/api/shifts?year=2026&month=3");
        var shifts = await response.Content.ReadFromJsonAsync<List<ShiftDto>>();

        shifts.Should().ContainSingle();
        shifts![0].Date.Month.Should().Be(3);
    }

    [Fact]
    public async Task DeleteShift_ByOwner_ReturnsNoContent()
    {
        var (client, cat) = await SetupAsync();

        var created = await client.PostAsJsonAsync("/api/shifts", new CreateShiftRequest(
            new DateOnly(2026, 3, 15), "07:00", "15:00", cat.Id, null, null));
        var shift = await created.Content.ReadFromJsonAsync<ShiftDto>();

        var response = await client.DeleteAsync($"/api/shifts/{shift!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
