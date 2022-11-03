using Discord;

namespace Cocotte.Utils;

public static class LogExtensions
{
    public static LogLevel ToLogLevel(this LogSeverity severity) => severity switch
    {
        LogSeverity.Critical => LogLevel.Critical,
        LogSeverity.Debug => LogLevel.Debug,
        LogSeverity.Error => LogLevel.Error,
        LogSeverity.Info => LogLevel.Information,
        LogSeverity.Verbose => LogLevel.Trace,
        LogSeverity.Warning => LogLevel.Warning,
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
    };

    public static void WriteToLogger<TLogger>(this LogMessage message, ILogger<TLogger> logger)
    {
        if (message.Severity == LogSeverity.Critical)
        {
            logger.LogCritical(message.Exception, "Discord.Net log from {source}: {message}", message.Source, message.Message);
        }
        else
        {
            logger.Log(message.Severity.ToLogLevel(), "Discord.Net log from {source}: {message}", message.Source, message.Message);
        }
    }
}