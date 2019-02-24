using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class GitRepository : IGitRepository
{
    private readonly IConfiguration _configuration;

    private readonly ILogger<IGitRepository> _logger;

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
            GitUser = _configuration["github:user"],
            GitPass = _configuration["github:pass"]
        };
    }

    public void Clone()
    {
        var settings = LoadSettings();
        var error = settings.Validate();
        if(error != null)
        {
            _logger.LogError($"GitRepository.Clone Failed to load settings: {error}");
            return;
        }

        try
        {
            var co = new CloneOptions();
            co.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = settings.GitUser, Password = settings.GitPass };
            co.Checkout = true;
            co.BranchName = "publish";
            Repository.Clone(settings.GitUrl, settings.LocalPath, co);
        }
        catch (System.Exception e)
        {
            _logger.LogError($"GitRepository.Clone Exception: {e}");
        }
    }

    public void Pull()
    {
        var settings = LoadSettings();
        var error = settings.Validate();
        if(error != null)
        {
            _logger.LogError($"GitRepository.Pull Failed to load settings: {error}");
            return;
        }

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
        catch (System.Exception e)
        {
            _logger.LogError($"GitRepository.Pull Exception: {e}");
        }
    }
}