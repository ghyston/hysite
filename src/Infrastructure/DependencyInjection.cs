using Microsoft.Extensions.DependencyInjection;
using HySite.Infrastructure.Services;
using HySite.Application.Interfaces; 
using HySite.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HySite.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IHysiteContext>(provider => provider.GetService<AppDbContext>()!);
        services.AddSingleton<IVersionService, VersionService>();
    }
    
}