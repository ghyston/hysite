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
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = "Host=hysite_db;Username=postgress;Password=password;Database=hysite_prod";
        
        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IHysiteContext>(provider => provider.GetService<AppDbContext>()!);
        services.AddSingleton<IVersionService, VersionService>();
    }
    
}