using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HySite.Application.Command;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HySite.Web.Services;

public class StartupService : IHostedService
{
    private readonly IConfiguration _config;
    private readonly ILogger<StartupService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public StartupService(IConfiguration config, ILogger<StartupService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _config = config;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var postsPath = _config["PostsLocalPath"];
        var configParsed = bool.TryParse(_config["loadFromGit"], out bool loadFromGit);

        if (!configParsed || !loadFromGit)
            return;

        if (postsPath is null)
        {
            _logger.LogWarning("Posts path not defined");
            return;
        }

        if (Directory.Exists(postsPath))
            Directory.Delete(postsPath, recursive: true);

        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetService<IMediator>();
        await mediator.Send(new CloneContentCmd());
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}