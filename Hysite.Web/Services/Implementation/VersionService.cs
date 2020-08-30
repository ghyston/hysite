using System;

namespace hySite
{
    public class VersionService : IVersionService
    {
        public string GetCurrentGitSHA()
        {
            var env = Environment.GetEnvironmentVariable("HYSITE_VERSION");
            return (env is null) 
                ? "unknown"
                : env.Substring(0, 7);
        }
    }
}
