using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using HySite.Application.Dto;
using HySite.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HySite.Application.Command;

public class UpdatePostsCommand : IRequest
{
    public required string Signature { get; set; }
    public required Stream Payload { get; set; }
}

//TODO: add tests
public class UpdatePostsCommandHandler : IRequestHandler<UpdatePostsCommand>
{
    private readonly IGitService _gitService;
    private readonly IConfiguration _configuration;
    private readonly IFileParserService _fileParserService;
    private readonly ILogger<UpdatePostsCommandHandler> _logger;

    public UpdatePostsCommandHandler(IGitService gitService, IConfiguration configuration, IFileParserService fileParserService, ILogger<UpdatePostsCommandHandler> logger)
    {
        _gitService = gitService;
        _configuration = configuration;
        _fileParserService = fileParserService;
        _logger = logger;
    }

    //TODO: common code to load git settings?
    private GitSettingsDto LoadSettings() => new GitSettingsDto
    {
        GitUrl = _configuration["PostsGitUrl"] ?? string.Empty,
        LocalPath = _configuration["PostsLocalPath"] ?? string.Empty,
        GitUser = _configuration["GithubUser"] ?? string.Empty,
        GithubToken = Environment.GetEnvironmentVariable("READER_TOKEN") 
            ?? _configuration["GithubToken"]
            ?? string.Empty
    };

    public async Task<Unit> Handle(UpdatePostsCommand request, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.Payload);

        var settings = LoadSettings();
        var payload = await reader.ReadToEndAsync();
        if (!_gitService.IsSecretValid(request.Signature, payload))
            throw new SecurityException(); //TODO: implement NotAuthorisedException, catch it in filter and return Http Unauthorized()

        var postsPath = _configuration["PostsLocalPath"];
        if(string.IsNullOrWhiteSpace(postsPath))
        {
            _logger.LogError($"Posts local path is not set");
            return Unit.Value;
        }

        var path = _configuration["PostsLocalPath"];
        var fileName = _configuration["RssFeedFile"];
        var rssPath = String.Join('/', path, fileName);

        _gitService.Pull(settings);
        _fileParserService.ParseExistingFiles(postsPath);
        //await _rssFeedService.CreateRssFeed(rssPath, cancellationToken); //TODO

        return Unit.Value;
    }
}