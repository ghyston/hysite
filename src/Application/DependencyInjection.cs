using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using HySite.Application.Dto;
using MediatR;
using HySite.Application.Command;
using HySite.Application.Interfaces;
using HySite.Application.Repositories;

namespace HySite.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(CloneContentCmd));
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssemblyContaining<GitSettingsDtoValidator>();
        services.AddScoped<IBlogPostRepository, BlogPostRepository>();
    }
}