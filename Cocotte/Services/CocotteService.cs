using System.Reflection;
using Cocotte.Modules.Activities;
using Cocotte.Options;
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
    private readonly ActivityOptions _activityOptions;
    private readonly InteractionService _interactionService;

    public CocotteService(ILogger<CocotteService> logger, IServiceProvider serviceProvider,
        IHostEnvironment hostEnvironment,
        IHostApplicationLifetime hostApplicationLifetime, DiscordSocketClient client,
        IOptions<DiscordOptions> options, IOptions<ActivityOptions> groupOptions, InteractionService interactionService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _hostApplicationLifetime = hostApplicationLifetime;
        _client = client;
        _options = options.Value;
        _activityOptions = groupOptions.Value;
        _interactionService = interactionService;
        _hostEnvironment = hostEnvironment;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Check token first
        if (string.IsNullOrWhiteSpace(_options.Token))
        {
            _logger.LogError("Couldn't find any discord bot token, exiting...");

            _hostApplicationLifetime.StopApplication();

            return;
        }

        if (!ValidateOptions())
        {
            return;
        }

        // Initialize modules and commands
        using var scope = _serviceProvider.CreateScope();
        #if DEBUG
        await _interactionService.AddModuleAsync(typeof(Modules.Ping.PingModule), scope.ServiceProvider);
        #endif
        await _interactionService.AddModuleAsync(typeof(ActivityModule), scope.ServiceProvider);

        _client.Ready += ClientOnReady;
        _client.InteractionCreated += HandleInteraction;

        // Start bot
        await _client.LoginAsync(TokenType.Bot, _options.Token);
        await _client.StartAsync();

        // Register events
        RegisterEvents();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void RegisterEvents()
    {
        var composteRolesListener = _serviceProvider.GetRequiredService<CompositeRolesListener>();

        _client.GuildMemberUpdated += composteRolesListener.UserUpdated;
    }

    private bool ValidateOptions()
    {
        // Validate group options
        if ((_activityOptions.HelperRoleId
            | _activityOptions.DpsRoleId
            | _activityOptions.TankRoleId
            | _activityOptions.SupportRoleId) == 0)
        {
            _logger.LogError("One of the group options id is invalid, it cannot be 0");

            return false;
        }

        return true;
    }

    private async Task ClientOnReady()
    {
        // Context & Slash commands can be automatically registered, but this process needs to happen after the client enters the READY state.
        // Since Global Commands take around 1 hour to register, we should use a test guild to instantly update and test our commands.
        if (_hostEnvironment.IsDevelopment())
        {
            // Check that a dev guild is set
            if (!_options.DevGuildId.HasValue && _options.DevGuildId!.Value != 0)
            {
                _logger.LogError("Couldn't find any dev guild while application is run in dev mode, exiting...");

                _hostApplicationLifetime.StopApplication();

                return;
            }
            await _interactionService.RegisterCommandsToGuildAsync(_options.DevGuildId.Value);
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
        _logger.LogTrace("[Interaction/Trace] Received interaction: by {User} in #{Channel} of type {Type}", interaction.User, interaction.Channel, interaction.Type);

        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            var context = new SocketInteractionContext(_client, interaction);

            // Execute the incoming command.
            var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

            if (!result.IsSuccess)
            {
                _logger.LogDebug("[Interaction/Trace] Error while executing interaction: {Interaction} in {Channel} because {Error}:{Reason}", interaction.Token, interaction.Channel, result.Error, result.ErrorReason);
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