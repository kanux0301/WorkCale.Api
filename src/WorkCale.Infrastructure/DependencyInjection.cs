using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkCale.Application.Services;
using WorkCale.Infrastructure.Auth;
using WorkCale.Infrastructure.Persistence;
using WorkCale.Infrastructure.Persistence.Repositories;

namespace WorkCale.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IShiftCategoryRepository, ShiftCategoryRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<ICalendarShareRepository, CalendarShareRepository>();

        // Auth services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IGoogleTokenVerifier, GoogleTokenVerifier>();

        return services;
    }
}
