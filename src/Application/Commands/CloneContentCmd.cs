using Microsoft.Extensions.Configuration;
using HySite.Application.Interfaces;
using MediatR;
using FluentValidation;
using HySite.Application.Dto;
using Microsoft.Extensions.Logging;

namespace HySite.Application.Command;

public class CloneContentCmd : IRequest
{
    
}

// TODO: add tests
public class CloneContentHandler : IRequestHandler<CloneContentCmd>
{
    private readonly IGitService _gitService;
    private readonly IConfiguration _configuration;
    private readonly IValidator<GitSettingsDto> _settingsValidator;
    private readonly IFileParserService _fileParserService;
    private readonly IHysiteContext _dbContext;
    private readonly ILogger<CloneContentHandler> _logger;

    public CloneContentHandler(IGitService service,
        IConfiguration configuration,
        IValidator<GitSettingsDto> settingsValidator,
        IFileParserService fileParserService,
        IHysiteContext hysiteContext,
        ILogger<CloneContentHandler> logger)
    {
        _gitService = service;
        _configuration = configuration;
        _settingsValidator = settingsValidator;
        _fileParserService = fileParserService;
        _dbContext = hysiteContext;
        _logger = logger;
    }

    private GitSettingsDto LoadSettings() => new GitSettingsDto
    {
        GitUrl = _configuration["PostsGitUrl"] ?? string.Empty,
        LocalPath = _configuration["PostsLocalPath"] ?? string.Empty,
        GitUser = _configuration["GithubUser"] ?? string.Empty,
        GithubToken = Environment.GetEnvironmentVariable("READER_TOKEN") 
            ?? _configuration["GithubToken"]
            ?? string.Empty
    };

    public async Task<Unit> Handle(CloneContentCmd request, CancellationToken cancellationToken)
    {
        var settings = LoadSettings();
        var result = await _settingsValidator.ValidateAsync(settings);

        if (!result.IsValid) 
        {
            var errors = string.Join(' ', result.Errors.Select(e => e.ErrorMessage));
            _logger.LogError($"Error in git settings: {errors}");
            return Unit.Value;    
        }

        _gitService.Clone(settings);

        var postsPath = _configuration["PostsLocalPath"];
        if(string.IsNullOrWhiteSpace(postsPath))
        {
            _logger.LogError($"Posts local path is not set");
            return Unit.Value;
        }

        var posts = _fileParserService.ParseExistingFiles(postsPath);
        if(!posts.Any())
        {
            _logger.LogError($"No posts have been loaded");
            return Unit.Value;
        }

        // TODO: do upsert, not a complete overwrite
        _dbContext.BlogPosts.RemoveRange(_dbContext.BlogPosts);
        _dbContext.BlogPosts.AddRange(posts);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}