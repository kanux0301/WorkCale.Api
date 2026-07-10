using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WorkCale.Api.IntegrationTests.Helpers;
using WorkCale.Application.DTOs;
using Xunit;

namespace WorkCale.Api.IntegrationTests.Jobs;

public class JobsEndpointTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    private async Task<HttpClient> AuthedClient()
    {
        var client = factory.CreateClient();
        var auth = await AuthHelper.RegisterAndLoginAsync(client, factory);
        client.SetBearerToken(auth.AccessToken);
        return client;
    }

    [Fact]
    public async Task GetJobs_AfterRegister_ReturnsOneDefaultJob()
    {
        var client = await AuthedClient();

        var jobs = await client.GetFromJsonAsync<List<JobDto>>("/api/jobs");

        jobs.Should().HaveCount(1);
        jobs![0].IsDefault.Should().BeTrue();
        jobs[0].Name.Should().Be("My Job");
    }

    [Fact]
    public async Task GetJobs_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/jobs");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateJob_ReturnsCreatedNonDefault()
    {
        var client = await AuthedClient();

        var response = await client.PostAsJsonAsync("/api/jobs",
            new CreateJobRequest("Hospital B", "#EC4899", "medkit"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<JobDto>();
        created!.Name.Should().Be("Hospital B");
        created.IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateJob_ChangesNameAndColor()
    {
        var client = await AuthedClient();
        var jobs = await client.GetFromJsonAsync<List<JobDto>>("/api/jobs");
        var jobId = jobs![0].Id;

        var response = await client.PutAsJsonAsync($"/api/jobs/{jobId}",
            new UpdateJobRequest("Renamed", "#123456", null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<JobDto>();
        updated!.Name.Should().Be("Renamed");
        updated.Color.Should().Be("#123456");
    }

    [Fact]
    public async Task ArchiveDefaultJob_Returns409()
    {
        var client = await AuthedClient();
        var jobs = await client.GetFromJsonAsync<List<JobDto>>("/api/jobs");

        var response = await client.PostAsync($"/api/jobs/{jobs![0].Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task MakeDefault_SwapsDefaultFlag()
    {
        var client = await AuthedClient();
        var newJob = (await (await client.PostAsJsonAsync("/api/jobs",
            new CreateJobRequest("Second", "#EC4899", null)))
            .Content.ReadFromJsonAsync<JobDto>())!;

        var response = await client.PostAsync($"/api/jobs/{newJob.Id}/make-default", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var jobs = (await client.GetFromJsonAsync<List<JobDto>>("/api/jobs"))!;
        jobs.Single(j => j.Id == newJob.Id).IsDefault.Should().BeTrue();
        jobs.Where(j => j.Id != newJob.Id).Should().OnlyContain(j => !j.IsDefault);
    }

    [Fact]
    public async Task CategoriesSeededOnRegister_AllReferenceDefaultJob()
    {
        var client = await AuthedClient();
        var jobs = (await client.GetFromJsonAsync<List<JobDto>>("/api/jobs"))!;
        var cats = (await client.GetFromJsonAsync<List<ShiftCategoryDto>>("/api/categories"))!;

        var defaultJobId = jobs.Single(j => j.IsDefault).Id;
        cats.Should().HaveCount(2);
        cats.Should().OnlyContain(c => c.JobId == defaultJobId);
    }

    [Fact]
    public async Task CategoriesFilteredByJob_ReturnsOnlyThatJob()
    {
        var client = await AuthedClient();
        var jobs = (await client.GetFromJsonAsync<List<JobDto>>("/api/jobs"))!;
        var defaultJobId = jobs.Single(j => j.IsDefault).Id;

        // Create a second job + a category on it.
        var secondJob = (await (await client.PostAsJsonAsync("/api/jobs",
            new CreateJobRequest("Second", "#EC4899", null)))
            .Content.ReadFromJsonAsync<JobDto>())!;
        await client.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequest(secondJob.Id, "Weekend", "#10B981", null, null));

        var filtered = await client.GetFromJsonAsync<List<ShiftCategoryDto>>($"/api/categories?jobId={defaultJobId}");

        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(c => c.JobId == defaultJobId);
    }
}
