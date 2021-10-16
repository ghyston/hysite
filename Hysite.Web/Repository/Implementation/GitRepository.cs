using System;
using System.Security.Cryptography;
using System.Text;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace hySite 
{
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
            public string GithubToken {get; set;}

            public string Validate()
            {
                if(string.IsNullOrWhiteSpace(GitUrl))
                    return "GitUrl is empty";
                    
                if(string.IsNullOrWhiteSpace(LocalPath))
                    return "LocalPath is empty";

                if(string.IsNullOrWhiteSpace(GitUser))
                    return "GitUser is empty";

                if(string.IsNullOrWhiteSpace(GithubToken))
                    return "GithubToken is empty";

                return null;
            }
        }

        public GitRepository(IConfiguration configuration, ILogger<IGitRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private SettingsDto LoadSettings() => new SettingsDto
        {
            GitUrl = _configuration["PostsGitUrl"],
            LocalPath = _configuration["PostsLocalPath"],
            GitUser = _configuration["GithubUser"],
            GithubToken = Environment.GetEnvironmentVariable("READER_TOKEN") ?? string.Empty
        };

        public IResult Clone()
        {
            _logger.LogInformation("Cloning repository..");
            
            var settings = LoadSettings();
            _logger.LogInformation($"Using credentials: {settings.GitUser} {settings.GithubToken}");
            var error = settings.Validate();
            if(error != null)
                return this.Error("Clone", $"Failed to load settings: {error}");

            try
            {
                var co = new CloneOptions();
                co.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = settings.GitUser, Password = settings.GithubToken };
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
                            Password = settings.GithubToken };

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
            var token = _configuration["GithubHookSecret"];

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
}