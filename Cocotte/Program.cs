using Cocotte.Modules.Activities;
using Cocotte.Modules.Activities.Models;
using Cocotte.Modules.Raids;
using Cocotte.Options;
using Cocotte.Services;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

DiscordSocketConfig discordSocketConfig = new()
{
    LogLevel = LogSeverity.Debug,
    MessageCacheSize = 200,
    GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.Guilds
};

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, configuration) =>
    {
        configuration.AddJsonFile("discord.json", false, false);
        configuration.AddJsonFile("activity.json", false, false);
        configuration.AddJsonFile("compositeRoles.json", false, false);
    })
    .ConfigureServices((context, services) =>
    {
        // Options
        services.Configure<DiscordOptions>(context.Configuration.GetSection(DiscordOptions.SectionName));
        services.Configure<ActivityOptions>(context.Configuration.GetSection(ActivityOptions.SectionName));
        services.Configure<CompositeRolesOptions>(context.Configuration.GetSection(CompositeRolesOptions.SectionName));

        // Database
        services.AddDbContext<CocotteDbContext>(options =>
            options.UseSqlite(context.Configuration.GetConnectionString("CocotteContext")), ServiceLifetime.Transient, ServiceLifetime.Transient);
        services.AddTransient<ActivitiesRepository>();

        // Discord.Net
        services.AddHostedService<DiscordLoggingService>();

        services.AddSingleton<CommandService>();
        services.AddSingleton(discordSocketConfig);
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));

        services.AddHostedService<CocotteService>();

        // Data
        services.AddSingleton<IRaidsRepository, MemoryRaidRepository>();
        services.AddSingleton<IPlayerInfosRepository, MemoryPlayerInfosRepository>();
        services.AddSingleton<RolesOptions>();

        // Activities
        services.AddTransient<ActivityFormatter>();
        services.AddTransient<InterstellarFormatter>();
        services.AddTransient<ActivityHelper>();

        // Composite roles
        services.AddSingleton<CompositeRolesListener>();

        // Raids
        services.AddTransient<RaidFormatter>();
        services.AddSingleton<RaidRegisterManager>();
        services.AddTransient<RosterAssigner>();

        // Custom
        services.AddSingleton<SharedCounter>();
        services.AddTransient<TransientCounter>();
    })
    .Build();

// Recreate database if in development environment
await using(var scope = host.Services.CreateAsyncScope())
{
    var hostEnvironment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

    var dbContext = scope.ServiceProvider.GetRequiredService<CocotteDbContext>();
    if (hostEnvironment.IsDevelopment())
    {
        // await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
    else
    {
        // Apply migrations
        await dbContext.Database.MigrateAsync();
    }
}

await host.RunAsync();