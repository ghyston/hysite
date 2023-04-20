using System;
using System.Reflection;
using System.Runtime.Versioning;
using HySite.Application.Interfaces;

namespace HySite.Infrastructure.Services;

public class VersionService : IVersionService
{
    private const string UNKNOWN = "unknown";

    public string GetCurrentGitSHA()
    {
        var env = Environment.GetEnvironmentVariable("HYSITE_VERSION");
        return (env is null) 
            ? UNKNOWN
            : env.Substring(0, Math.Min(7, env.Length));
    }

    public string GetFrameworkVersion() => 
        Assembly
        .GetEntryAssembly()?
        .GetCustomAttribute<TargetFrameworkAttribute>()?
        .FrameworkName ?? UNKNOWN;
}
