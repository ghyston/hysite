using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class GitRepository : IGitRepository
{
    private readonly IConfiguration _configuration;

    private readonly ILogger<IGitRepository> _logger;

    private const string Sha1Prefix = "sha1=";

    private class SettingsDto
    {
        public string GitUrl {get; set;}
        public string LocalPath {get; set;}
        public string GitUser {get; set;}
        public string GitPass {get; set;}

        public string Validate()
        {
            if(string.IsNullOrWhiteSpace(GitUrl))
                return "GitUrl is empty";
                
            if(string.IsNullOrWhiteSpace(LocalPath))
                return "LocalPath is empty";

            if(string.IsNullOrWhiteSpace(GitUser))
                return "GitUser is empty";

            if(string.IsNullOrWhiteSpace(GitPass))
                return "GitPass is empty";

            return null;
        }
    }

    public GitRepository(IConfiguration configuration, ILogger<IGitRepository> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private SettingsDto LoadSettings()
    {
        return new SettingsDto
        {
            GitUrl = _configuration["PostsGitUrl"],
            LocalPath = _configuration["PostsLocalPath"],
            GitUser = _configuration["github-user"],
            GitPass = _configuration["github-pass"]
        };
    }

    public IResult Clone()
    {
        _logger.LogInformation("Cloning repository..");
        
        var settings = LoadSettings();
        var error = settings.Validate();
        if(error != null)
            return this.Error("Clone", $"Failed to load settings: {error}");

        try
        {
            var co = new CloneOptions();
            co.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = settings.GitUser, Password = settings.GitPass };
            co.Checkout = true;
            co.BranchName = "publish";
            Repository.Clone(settings.GitUrl, settings.LocalPath, co);
        }
        catch (System.Exception exception)
        {
            return this.Error("Clone", $"Exception: {exception}");
        }
        return Result.Success();
    }

    public IResult Pull()
    {
        var settings = LoadSettings();
        var error = settings.Validate();
        if(error != null) 
            return this.Error("Pull", $"Failed to load settings: {error}");

        try
        {
            using (var repo = new Repository(settings.LocalPath))
            {
                LibGit2Sharp.PullOptions options = new LibGit2Sharp.PullOptions();
                options.FetchOptions = new FetchOptions();
                options.FetchOptions.CredentialsProvider = (_url, _user, _cred) => 
                    new UsernamePasswordCredentials { 
                        Username = settings.GitUser, 
                        Password = settings.GitPass };

                var signature = new LibGit2Sharp.Signature(new Identity(settings.GitUser, settings.GitUser), DateTime.Now);
                Commands.Pull(repo, signature, options);
            }            
        }
        catch (System.Exception exception)
        {
            return this.Error("Pull", $"Exception: {exception}");
        }
        return Result.Success();
    }

    // mostly copied from https://www.jerriepelser.com/blog/create-github-webhook-aspnetcore-aws-lambda/
    public bool IsSecretValid(string signatureWithPrefix, string payload)
    {
        var token = _configuration["github-hookSecret"];

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

        if (signatureWithPrefix.StartsWith(Sha1Prefix, StringComparison.OrdinalIgnoreCase))
        {
            var signature = signatureWithPrefix.Substring(Sha1Prefix.Length);
            var secret = Encoding.ASCII.GetBytes(token);
            var payloadBytes = Encoding.ASCII.GetBytes(payload);

            using (var hmacsha1 = new HMACSHA1(secret))
            {
                var hash = hmacsha1.ComputeHash(payloadBytes);
                var hashString = ToHexString(hash);
                return (hashString.Equals(signature));
            }
        }

        return false;
    }

    private static string ToHexString(byte[] bytes)
    {
        StringBuilder builder = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            builder.AppendFormat("{0:x2}", b);
        }

        return builder.ToString();
    }

    private IResult Error(string method, string message)
    {
        var messageWithPath = $"GitRepository.{method}: {message}";
        _logger.LogError(messageWithPath);
        return Result.Error(messageWithPath);
    }
}