using Discord.Interactions;

namespace Cocotte.Modules;

public class PingModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<PingModule> _logger;

    public PingModule(ILogger<PingModule> logger)
    {
        _logger = logger;
    }

    [SlashCommand("ping", "Check if Coco is alive")]
    public async Task Ping()
    {
        await RespondAsync($":ping_pong: It took me {Context.Client.Latency}ms to respond to you!", ephemeral: true);
    }
}