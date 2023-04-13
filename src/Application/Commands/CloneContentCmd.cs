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
    private readonly ILogger<CloneContentHandler> _logger;

    public CloneContentHandler(IGitService service,
        IConfiguration configuration,
        IValidator<GitSettingsDto> settingsValidator,
        ILogger<CloneContentHandler> logger)
    {
        _gitService = service;
        _configuration = configuration;
        _settingsValidator = settingsValidator;
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

        return Unit.Value;
    }
}