using System.Reflection;
using Cocotte.Options;
using Cocotte.Utils;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Cocotte.Services;

public class CocotteService : BackgroundService
{
    private readonly ILogger<CocotteService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly DiscordSocketClient _client;
    private readonly DiscordOptions _options;
    private readonly InteractionService _interactionService;

    public CocotteService(ILogger<CocotteService> logger, IServiceProvider serviceProvider,
        IHostEnvironment hostEnvironment,
        IHostApplicationLifetime hostApplicationLifetime, DiscordSocketClient client,
        IOptions<DiscordOptions> options, InteractionService interactionService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _hostApplicationLifetime = hostApplicationLifetime;
        _client = client;
        _options = options.Value;
        _interactionService = interactionService;
        _hostEnvironment = hostEnvironment;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Check token first
        if (_options.Token is null)
        {
            _logger.LogError("Couldn't find any discord bot token, exiting...");

            _hostApplicationLifetime.StopApplication();

            return;
        }

        // Initialize modules and commands
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

        _client.Ready += ClientOnReady;
        _client.InteractionCreated += HandleInteraction;

        // Start bot
        await _client.LoginAsync(TokenType.Bot, _options.Token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ClientOnReady()
    {
        // Context & Slash commands can be automatically registered, but this process needs to happen after the client enters the READY state.
        // Since Global Commands take around 1 hour to register, we should use a test guild to instantly update and test our commands.
        if (_hostEnvironment.IsDevelopment())
        {
            // Check that a dev guild is set
            if (!_options.DevGuildId.HasValue)
            {
                _logger.LogError("Couldn't find any dev guild while application is run in dev mode, exiting...");

                _hostApplicationLifetime.StopApplication();

                return;
            }
            await _interactionService.RegisterCommandsToGuildAsync(_options.DevGuildId.Value, true);
        }
        else
        {
            await _interactionService.RegisterCommandsGloballyAsync();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        _logger.LogTrace("[Interaction/Trace] Received interaction: by {user} in #{channel} of type {type}", interaction.User, interaction.Channel, interaction.Type);

        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            var context = new SocketInteractionContext(_client, interaction);

            // Execute the incoming command.
            var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

            if (!result.IsSuccess)
            {
                _logger.LogDebug("[Interaction/Trace] Error while executing interaction: {interaction} in {channel}", interaction.Token, interaction.Channel);

                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                }
            }
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
            {
                await interaction.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync());
            }
        }
    }
}