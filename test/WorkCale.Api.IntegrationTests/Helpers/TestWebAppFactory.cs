using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WorkCale.Infrastructure.Persistence;

namespace WorkCale.Api.IntegrationTests.Helpers;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "workcale_test_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // ConfigureTestServices runs AFTER the app's services, so our registration wins
        builder.ConfigureTestServices(services =>
        {
            // Remove all DbContext-related registrations from the real app
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();

            // Build an isolated EF service provider with only InMemory services
            // This avoids the dual provider conflict with Npgsql still in the app DI
            var efServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Re-add AppDbContext using InMemory with isolated internal service provider
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
                options.UseInternalServiceProvider(efServiceProvider);
            });
        });
    }
}
