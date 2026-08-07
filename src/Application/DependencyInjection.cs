using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using HySite.Application.Dto;
using HySite.Application.Command;
using HySite.Application.Interfaces;
using HySite.Application.Repositories;

namespace HySite.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CloneContentCmd).Assembly));
        services.AddValidatorsFromAssemblyContaining<GitSettingsDtoValidator>();
        services.AddScoped<IBlogPostRepository, BlogPostRepository>();
    }
}
