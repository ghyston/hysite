using System;
using System.Reflection;
using System.Runtime.Versioning;
using HySite.Application.Interfaces;

namespace HySite.Infrastructure.Services;

public class VersionService : IVersionService
{
    public string GetCurrentGitSHA()
    {
        var env = Environment.GetEnvironmentVariable("HYSITE_VERSION");
        return (env is null) 
            ? "unknown"
            : env.Substring(0, Math.Min(7, env.Length));
    }

    public string GetFrameworkVersion() => 
        Assembly
        .GetEntryAssembly()?
        .GetCustomAttribute<TargetFrameworkAttribute>()?
        .FrameworkName;
}
