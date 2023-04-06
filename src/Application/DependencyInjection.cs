using System.Reflection;
using FluentValidation;
using HySite.Application.Command;
using HySite.Application.Dto;
using HySite.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HySite.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssemblyContaining<GitSettingsDtoValidator>();
    }
}