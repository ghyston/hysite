using Microsoft.Extensions.DependencyInjection;
using HySite.Infrastructure.Services;
using HySite.Application.Interfaces; 
using HySite.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HySite.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        var envConnectionStr = Environment.GetEnvironmentVariable("HYSITE_DB_CONNECTION");
        var connectionString = string.IsNullOrWhiteSpace(envConnectionStr) 
            ? configuration.GetConnectionString("Database")
            : envConnectionStr;

        logger.LogInformation($"Try to connect db {connectionString}");
        
        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IHysiteContext>(provider => provider.GetService<AppDbContext>()!);
        services.AddSingleton<IVersionService, VersionService>();
    }
    
}