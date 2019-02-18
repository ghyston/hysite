using System;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;

public class GitRepository : IGitRepository
{
    private readonly IConfiguration _configuration;

    public GitRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Clone()
    {
        var remoteUrl = _configuration["PostsGitUrl"];
        var localPath = _configuration["PostsLocalPath"];

        try
        {
            var co = new CloneOptions();
            //@todo: store it in secrets!
            co.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = "xxx", Password = "xxx" };
            //Repository.Clone("https://github.com/libgit2/libgit2sharp.git", "path/to/repo", co);
            co.Checkout = true;
            Repository.Clone(remoteUrl, localPath, co);
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Pull()
    {
        //@todo
    }
}