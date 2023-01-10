namespace HySite.Application.Interfaces;

public interface IVersionService
{
    string GetCurrentGitSHA();
    string GetFrameworkVersion();
}
