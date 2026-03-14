using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WorkCale.Api.IntegrationTests.Helpers;
using WorkCale.Application.DTOs;
using Xunit;

namespace WorkCale.Api.IntegrationTests.Categories;

public class CategoriesEndpointTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(HttpClient client, string token, List<ShiftCategoryDto> cats)> SetupAsync()
    {
        var client = factory.CreateClient();
        var auth = await AuthHelper.RegisterAndLoginAsync(client);
        client.SetBearerToken(auth.AccessToken);
        var cats = await client.GetFromJsonAsync<List<ShiftCategoryDto>>("/api/categories");
        return (client, auth.AccessToken, cats!);
    }

    // ── GET /api/categories ────────────────────────────────────────────────

    [Fact]
    public async Task GetCategories_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/categories");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCategories_AfterRegister_ReturnsTwoDefaultCategories()
    {
        var (client, _, cats) = await SetupAsync();

        cats.Should().HaveCount(2);
        cats.Should().Contain(c => c.Name == "Day Shift");
        cats.Should().Contain(c => c.Name == "Night Shift");
    }

    // ── POST /api/categories ───────────────────────────────────────────────

    [Fact]
    public async Task CreateCategory_WithValidData_Returns201()
    {
        var (client, _, _) = await SetupAsync();

        var response = await client.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequest("Evening Shift", "#10B981", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var cat = await response.Content.ReadFromJsonAsync<ShiftCategoryDto>();
        cat!.Name.Should().Be("Evening Shift");
        cat.Color.Should().Be("#10B981");
    }

    [Fact]
    public async Task CreateCategory_WithMissingName_Returns400()
    {
        var (client, _, _) = await SetupAsync();

        var response = await client.PostAsJsonAsync("/api/categories",
            new { name = (string?)null, color = "#10B981" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCategory_WithMissingColor_Returns400()
    {
        var (client, _, _) = await SetupAsync();

        var response = await client.PostAsJsonAsync("/api/categories",
            new { name = "Shift", color = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── PUT /api/categories/:id ────────────────────────────────────────────

    [Fact]
    public async Task UpdateCategory_WithValidData_Returns200()
    {
        var (client, _, cats) = await SetupAsync();
        var catId = cats.First().Id;

        var response = await client.PutAsJsonAsync($"/api/categories/{catId}",
            new UpdateCategoryRequest("Updated Name", "#ABCDEF", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ShiftCategoryDto>();
        updated!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateCategory_NotFound_Returns404()
    {
        var (client, _, _) = await SetupAsync();

        var response = await client.PutAsJsonAsync($"/api/categories/{Guid.NewGuid()}",
            new UpdateCategoryRequest("Name", "#123456", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCategory_OwnedByOtherUser_Returns401()
    {
        var (clientA, _, catsA) = await SetupAsync();
        var (clientB, _, _) = await SetupAsync();

        var response = await clientB.PutAsJsonAsync($"/api/categories/{catsA.First().Id}",
            new UpdateCategoryRequest("Stolen", "#000000", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── DELETE /api/categories/:id ─────────────────────────────────────────

    [Fact]
    public async Task DeleteCategory_WithNoShifts_Returns204()
    {
        var (client, _, cats) = await SetupAsync();
        var catId = cats.First().Id;

        var response = await client.DeleteAsync($"/api/categories/{catId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteCategory_NotFound_Returns404()
    {
        var (client, _, _) = await SetupAsync();

        var response = await client.DeleteAsync($"/api/categories/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_WithExistingShifts_Returns409()
    {
        var (client, _, cats) = await SetupAsync();
        var catId = cats.First().Id;

        await client.PostAsJsonAsync("/api/shifts",
            new CreateShiftRequest(new DateOnly(2026, 3, 1), "08:00", "16:00", catId, null, null));

        var response = await client.DeleteAsync($"/api/categories/{catId}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
