using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Serilog;
using Serilog.Events;

namespace HySite.Web.Services;

public class LoggingConfigurator
{
    public void Configure(ILoggingBuilder logging, IConfiguration configuration, bool isDevelopment)
    {
        logging.ClearProviders();
        logging.AddConsole();

        if (isDevelopment)
        {
            logging.AddFilter<ConsoleLoggerProvider>(null, LogLevel.Trace);
            return;
        }

        var logsPath = configuration["LogsLocalPath"] ?? "logs";
        var logsFullPath = Path.GetFullPath(logsPath);
        Directory.CreateDirectory(logsFullPath);

        var informationDays = configuration.GetValue("LogRetention:InformationDays", 2);
        var warningDays = configuration.GetValue("LogRetention:WarningAndAboveDays", 60);
        if (informationDays <= 0 || warningDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(configuration), "Log retention periods must be positive numbers of days.");

        var logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(Path.Combine(logsFullPath, "hysite-all-.log"),
                restrictedToMinimumLevel: LogEventLevel.Information,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: null,
                retainedFileTimeLimit: TimeSpan.FromDays(informationDays))
            .WriteTo.File(Path.Combine(logsFullPath, "hysite-warnings-.log"),
                restrictedToMinimumLevel: LogEventLevel.Warning,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: null,
                retainedFileTimeLimit: TimeSpan.FromDays(warningDays))
            .CreateLogger();

        logging.AddSerilog(logger, dispose: true);
    }
}
