using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HySite.Application;
using HySite.Application.Interfaces;
using HySite.Infrastructure;
using HySite.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var logsPath = builder.Configuration["LogsLocalPath"];
var logsFullPath = EnsureDirectoryExist(Directory.GetCurrentDirectory(), logsPath);
builder.Logging.AddFile(logsFullPath + "/hysite-{Date}.log");

// Configure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebPresentation();

var app = builder.Build();

var versionService = app.Services.GetService<IVersionService>();
var config = app.Services.GetService<IConfiguration>();

using var scope = app.Services.CreateScope();

app.Logger.LogInformation($"Application builded in {app.Environment.EnvironmentName} environment from commit {versionService.GetCurrentGitSHA()}! 🛠️");

scope.MigrateDatabase();

CheckCertificateDirectories(app.Configuration, app.Logger);

var postsPath = app.Configuration["PostsLocalPath"];
var postsFullPath = EnsureDirectoryExist(Directory.GetCurrentDirectory(), postsPath);
var imagesFullPath = EnsureDirectoryExist(postsFullPath, "img");

app.SetupRouting(postsFullPath, imagesFullPath);

app.Run();

#region Startup helper functions

void CheckCertificateDirectories(IConfiguration config, ILogger logger)
{
    var fullchainPath = config["Kestrel:Certificates:Default:Path"] ?? string.Empty;
    var privkeyPath = config["Kestrel:Certificates:Default:KeyPath"] ?? string.Empty;
    var certPath = config["CertPath"] ?? string.Empty;

    if (!File.Exists(fullchainPath))
        logger.LogWarning($"Certificate file ({fullchainPath}) does NOT exist!");

    if (Directory.Exists(certPath))
    {
        var files = Directory.GetFiles(certPath);
        logger.LogInformation("Cert folder content:");
        foreach (var file in files)
            logger.LogInformation(file);
    }

    logger.LogInformation($"Cert path: {certPath}");
    logger.LogInformation($"Kestrel path: {fullchainPath}");
    logger.LogInformation($"Kestrel keypath: {privkeyPath}");
}

string EnsureDirectoryExist(params string[] subPaths)
{
	var fullPath = Path.Combine(subPaths);

	if (!Directory.Exists(fullPath))
		Directory.CreateDirectory(fullPath);

	return fullPath;
}

#endregion