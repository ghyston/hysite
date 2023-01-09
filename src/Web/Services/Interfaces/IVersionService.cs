namespace hySite
{
    public interface IVersionService 
    {
        string GetCurrentGitSHA();
        string GetFrameworkVersion();
    }
}