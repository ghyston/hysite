using FluentAssertions;
using HySite.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Web.UnitTests;

public class LoggingConfiguratorTests
{
    [Fact]
    public void ProductionWritesInformationAndAboveToShortLivedFilesAndWarningsToBoth()
    {
        var logsPath = Path.Combine(Path.GetTempPath(), $"hysite-logging-{Guid.NewGuid()}");
        try
        {
            var configuration = CreateConfiguration(logsPath, "Production");
            using (var factory = CreateFactory(configuration, false))
            {
                var logger = factory.CreateLogger("Test");
                logger.LogDebug("debug message");
                logger.LogInformation("information message");
                factory.CreateLogger("Microsoft.Test").LogInformation("framework information");
                logger.LogWarning("warning message");
                logger.LogError("error message");
                logger.LogCritical("critical message");
            }

            var allLogs = File.ReadAllText(Directory.GetFiles(logsPath, "hysite-all-*.log").Should().ContainSingle().Subject);
            var warningLogs = File.ReadAllText(Directory.GetFiles(logsPath, "hysite-warnings-*.log").Should().ContainSingle().Subject);
            allLogs.Should().NotContain("debug message").And.Contain("information message").And.Contain("framework information")
                .And.Contain("warning message").And.Contain("error message").And.Contain("critical message");
            warningLogs.Should().NotContain("information message").And.Contain("warning message")
                .And.Contain("error message").And.Contain("critical message");
        }
        finally
        {
            if (Directory.Exists(logsPath))
                Directory.Delete(logsPath, true);
        }
    }

    [Theory]
    [InlineData(3, 20, false, true)]
    [InlineData(20, 3, true, false)]
    public void RetentionUsesConfiguredAgesAndLeavesLegacyFilesAlone(int informationDays, int warningDays, bool keepAll, bool keepWarnings)
    {
        var logsPath = Path.Combine(Path.GetTempPath(), $"hysite-logging-{Guid.NewGuid()}");
        Directory.CreateDirectory(logsPath);
        try
        {
            var oldDate = DateTime.Now.AddDays(-10).ToString("yyyyMMdd");
            var oldAll = Path.Combine(logsPath, $"hysite-all-{oldDate}.log");
            var oldWarnings = Path.Combine(logsPath, $"hysite-warnings-{oldDate}.log");
            var legacy = Path.Combine(logsPath, $"hysite-{oldDate}.log");
            File.WriteAllText(oldAll, "old");
            File.WriteAllText(oldWarnings, "old");
            File.WriteAllText(legacy, "legacy");

            var configuration = CreateConfiguration(logsPath, "Production", informationDays, warningDays);
            using (var factory = CreateFactory(configuration, false))
                factory.CreateLogger("Test").LogWarning("trigger retention");

            File.Exists(oldAll).Should().Be(keepAll);
            File.Exists(oldWarnings).Should().Be(keepWarnings);
            File.Exists(legacy).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(logsPath, true);
        }
    }

    [Fact]
    public void DevelopmentEnablesTraceForFrameworkCategoriesWithoutCreatingLogFiles()
    {
        var logsPath = Path.Combine(Path.GetTempPath(), $"hysite-logging-{Guid.NewGuid()}");
        var configuration = CreateConfiguration(logsPath, "Development");
        using var factory = CreateFactory(configuration, true);

        factory.CreateLogger("Microsoft.Test").IsEnabled(LogLevel.Trace).Should().BeTrue();
        factory.CreateLogger("System.Test").IsEnabled(LogLevel.Debug).Should().BeTrue();
        Directory.Exists(logsPath).Should().BeFalse();
    }

    private static IConfiguration CreateConfiguration(string logsPath, string environment, int informationDays = 2, int warningDays = 60) =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LogsLocalPath"] = logsPath,
                ["LogRetention:InformationDays"] = informationDays.ToString(),
                ["LogRetention:WarningAndAboveDays"] = warningDays.ToString(),
                ["Logging:LogLevel:Microsoft"] = "Information",
                ["Logging:LogLevel:System"] = "Information"
            })
            .Build();

    private static ILoggerFactory CreateFactory(IConfiguration configuration, bool isDevelopment) =>
        LoggerFactory.Create(logging =>
        {
            logging.AddConfiguration(configuration.GetSection("Logging"));
            new LoggingConfigurator().Configure(logging, configuration, isDevelopment);
        });
}
