using Cocotte.Modules.Raids;
using Cocotte.Options;
using Cocotte.Services;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

DiscordSocketConfig discordSocketConfig = new()
{
    LogLevel = LogSeverity.Debug,
    MessageCacheSize = 200
};

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, configuration) =>
    {
        configuration.AddJsonFile("discord.json", false, false);
    })
    .ConfigureServices((context, services) =>
    {
        // Options
        services.Configure<DiscordOptions>(context.Configuration.GetSection(DiscordOptions.SectionName));

        // Discord.Net
        services.AddHostedService<DiscordLoggingService>();

        services.AddSingleton<CommandService>();
        services.AddSingleton(discordSocketConfig);
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));

        services.AddHostedService<CocotteService>();

        // Data
        services.AddSingleton<IRaidsRepository, MemoryRaidRepository>();

        // Custom
        services.AddSingleton<SharedCounter>();
        services.AddTransient<TransientCounter>();
    })
    .Build();

await host.RunAsync();