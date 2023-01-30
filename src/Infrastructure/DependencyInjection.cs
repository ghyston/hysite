using Microsoft.Extensions.DependencyInjection;
using HySite.Infrastructure.Services;
using HySite.Application.Interfaces; 
using HySite.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace HySite.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        var connectionString = "Host=localhost;Username=maingalaxyroot;Password=themostcomplexone;Database=hysite_local";
        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IHysiteContext>(provider => provider.GetService<AppDbContext>()!);
        services.AddSingleton<IVersionService, VersionService>();
    }
    
}