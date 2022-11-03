using Cocotte.Utils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Cocotte.Services;

public class DiscordLoggingService : IHostedService
{
    private readonly ILogger<DiscordLoggingService> _logger;
    private readonly DiscordSocketClient _client;
    private readonly CommandService _command;

    public DiscordLoggingService(ILogger<DiscordLoggingService> logger, DiscordSocketClient client,
        CommandService command)
    {
        _logger = logger;
        _client = client;
        _command = command;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Log += LogAsync;
        _command.Log += LogAsync;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _client.Log -= LogAsync;
        _command.Log -= LogAsync;

        return Task.CompletedTask;
    }

    private Task LogAsync(LogMessage message)
    {
        if (message.Exception is CommandException cmdException)
        {
            _logger.Log(message.Severity.ToLogLevel(), cmdException,
                "[Command/{severity}] ({source}) {commandName} failed to execute in {channel}.",
                message.Source,
                message.Severity,
                cmdException.Command.Aliases.First(),
                cmdException.Context.Channel);
        }
        else
        {
            _logger.Log(message.Severity.ToLogLevel(), message.Exception, "[General/{severity}] ({source}) {message}", message.Severity, message.Source, message.Message);
        }

        return Task.CompletedTask;
    }
}