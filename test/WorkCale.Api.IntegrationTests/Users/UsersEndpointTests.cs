using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WorkCale.Api.IntegrationTests.Helpers;
using WorkCale.Application.DTOs;
using Xunit;

namespace WorkCale.Api.IntegrationTests.Users;

public class UsersEndpointTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    [Fact]
    public async Task Search_WithQueryTooShort_ReturnsEmptyList()
    {
        var client = factory.CreateClient();
        var auth = await AuthHelper.RegisterAndLoginAsync(client);
        client.SetBearerToken(auth.AccessToken);

        var response = await client.GetAsync("/api/users/search?q=a");
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

        users.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_WithMatchingQuery_ReturnsResults()
    {
        var uid = Guid.NewGuid().ToString("N")[..8];
        var targetClient = factory.CreateClient();
        await AuthHelper.RegisterAndLoginAsync(targetClient, $"find-{uid}@example.com", $"Find{uid}");

        var client = factory.CreateClient();
        var auth = await AuthHelper.RegisterAndLoginAsync(client);
        client.SetBearerToken(auth.AccessToken);

        var users = await client.GetFromJsonAsync<List<UserDto>>($"/api/users/search?q=Find{uid}");

        users.Should().ContainSingle(u => u.DisplayName == $"Find{uid}");
    }

    [Fact]
    public async Task Search_ExcludesRequestingUserFromResults()
    {
        var uid = Guid.NewGuid().ToString("N")[..8];
        var client = factory.CreateClient();
        var auth = await AuthHelper.RegisterAndLoginAsync(client, $"self-{uid}@example.com", $"Self{uid}");
        client.SetBearerToken(auth.AccessToken);

        var users = await client.GetFromJsonAsync<List<UserDto>>($"/api/users/search?q=Self{uid}");

        users.Should().NotContain(u => u.Id == auth.User.Id);
    }

    [Fact]
    public async Task Search_WithNoMatches_ReturnsEmptyList()
    {
        var client = factory.CreateClient();
        var auth = await AuthHelper.RegisterAndLoginAsync(client);
        client.SetBearerToken(auth.AccessToken);

        var users = await client.GetFromJsonAsync<List<UserDto>>(
            "/api/users/search?q=zzznomatchpossible999");

        users.Should().BeEmpty();
    }
}
