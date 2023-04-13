using FluentValidation;
using HySite.Application.Dto;
using HySite.Application.Interfaces;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HySite.Infrastructure.Services;

public class GitService : IGitService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GitService> _logger;

    public GitService(IConfiguration configuration, ILogger<GitService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // TODO: where is it used?
    private const string Sha1Prefix = "sha1=";

    public void Clone(GitSettingsDto settings)
    {
        _logger.LogInformation("Cloning repository..");

        try
        {
            var co = new CloneOptions();
            co.CredentialsProvider =
                (_url, _user, _cred) => new UsernamePasswordCredentials
                {
                    Username = settings.GitUser,
                    Password = settings.GithubToken
                };
            co.Checkout = true;
            co.BranchName = "publish";
            Repository.Clone(settings.GitUrl, settings.LocalPath, co);
        }
        catch (System.Exception exception)
        {
            _logger.LogError(exception, $"Error on cloning `{settings.GitUrl}`");
            return;
        }

        _logger.LogInformation("Repository cloned!");
    }
}