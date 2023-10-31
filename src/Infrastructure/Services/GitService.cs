using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using HySite.Application.Dto;
using HySite.Application.Interfaces;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HySite.Application.Extensions;

namespace HySite.Infrastructure.Services;

public class GitService : IGitService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GitService> _logger;

    private const string Sha1Prefix = "sha1=";

    public GitService(IConfiguration configuration, ILogger<GitService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void Clone(GitSettingsDto settings)
    {
        _logger.LogInformation("Cloning repository..");

        try
        {
            var co = new CloneOptions
            {
                CredentialsProvider = Credentials(settings),
                Checkout = true,
                BranchName = "publish"
            };
            Repository.Clone(settings.GitUrl, settings.LocalPath, co);
        }
        catch (System.Exception exception)
        {
            _logger.LogError(exception, "Error on cloning `{GitUrl}`", settings.GitUrl);
            return;
        }

        _logger.LogInformation("Repository cloned!");
    }

    public void Pull(GitSettingsDto settings)
    {
        _logger.LogInformation("Pull repository..");

        try
        {
            using var repo = new Repository(settings.LocalPath);
            LibGit2Sharp.PullOptions options = new()
            {
                FetchOptions = new FetchOptions()
            };
            options.FetchOptions.CredentialsProvider = Credentials(settings);

            var signature = new LibGit2Sharp.Signature(new Identity(settings.GitUser, settings.GitUser), DateTime.Now);
            Commands.Pull(repo, signature, options);
        }
        catch (System.Exception exception)
        {
            _logger.LogError(exception, "Error on pull updates `{GitUrl}`", settings.GitUrl);
            return;
        }

        _logger.LogInformation("Updates pulled!");
    }

    // mostly copied from https://www.jerriepelser.com/blog/create-github-webhook-aspnetcore-aws-lambda/
    public bool IsSecretValid(string signatureWithPrefix, string payload)
    {
        var token = _configuration["GithubHookSecret"];

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogError($"GithubHookSecret is not set");
            return false;
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            _logger.LogError($"GitRepository.IsSecretValid Payload is empty");
            return false;
        }

        if (string.IsNullOrWhiteSpace(signatureWithPrefix))
        {
            _logger.LogError($"GitRepository.IsSecretValid Signature is empty");
            return false;
        }

        if (!signatureWithPrefix.StartsWith(Sha1Prefix, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError($"GitRepository.IsSecretValid signature doesn't start with sha1 prefix");
            return false;
        }
        
        var signature = signatureWithPrefix.Substring(Sha1Prefix.Length);
        var secret = Encoding.ASCII.GetBytes(token);
        var payloadBytes = Encoding.ASCII.GetBytes(payload);

        using var hmacsha1 = new HMACSHA1(secret);
        
        var hash = hmacsha1.ComputeHash(payloadBytes);
        var hashString = hash.ConvertToString();
        return (hashString.Equals(signature));
        
    }

    private static LibGit2Sharp.Handlers.CredentialsHandler Credentials(GitSettingsDto settings) =>
        (_url, _user, _cred) =>
            new UsernamePasswordCredentials
            {
                Username = settings.GitUser,
                Password = settings.GithubToken
            };
}