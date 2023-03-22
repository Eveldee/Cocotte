using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Cocotte.Modules.Activities.Models;
using Cocotte.Options;
using Cocotte.Utils;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Cocotte.Modules.Activities;

/// <summary>
/// Module to ask and propose groups for different activities: Abyss, OOW, FC, ...
/// </summary>
[Group("activite", "Organise des activités")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public partial class ActivityModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<ActivityModule> _logger;
    private readonly ActivityOptions _options;
    private readonly ActivityHelper _activityHelper;
    private readonly ActivitiesRepository _activitiesRepository;
    private readonly ActivityFormatter _activityFormatter;

    public ActivityModule(ILogger<ActivityModule> logger, IOptions<ActivityOptions> options, ActivityHelper activityHelper, ActivitiesRepository activitiesRepository, ActivityFormatter activityFormatter)
    {
        _logger = logger;
        _activityHelper = activityHelper;
        _activitiesRepository = activitiesRepository;
        _activityFormatter = activityFormatter;
        _options = options.Value;
    }

    [RequireOwner]
    [SlashCommand("setup-info", "Display activity setup info")]
    public async Task SetupInfo()
    {
        await RespondAsync($"""
        - Helper: {MentionUtils.MentionRole(_options.HelperRoleId)} {_options.HelperEmote.ToEmote()}
        - Dps: {MentionUtils.MentionRole(_options.DpsRoleId)} {_options.DpsEmote.ToEmote()}
        - Tank: {MentionUtils.MentionRole(_options.TankRoleId)} {_options.TankEmote.ToEmote()}
        - Healer: {MentionUtils.MentionRole(_options.SupportRoleId)} {_options.SupportEmote.ToEmote()}
        """);
    }

    [SlashCommand("abime", "Créer un groupe pour l'Abîme du Néant")]
    public async Task ActivityAbyss([Summary("étage", "A quel étage êtes vous")] uint stage, [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        const ActivityName activityName = ActivityName.OriginsOfWar;
        var activityType = ActivityHelper.ActivityNameToType(activityName);
        var maxPlayers = ActivityHelper.ActivityTypeToMaxPlayers(activityType);

        var activity = new StagedActivity
        {
            ActivityId = 0,
            CreatorDiscordId = Context.User.Id,
            CreatorDiscordName = ((SocketGuildUser)Context.User).DisplayName,
            Description = description,
            Type = activityType,
            Name = activityName,
            MaxPlayers = maxPlayers,
            Stage = stage
        };

        await CreateRoleActivity(activity);
    }

    private async Task CreateRoleActivity(Activity activity)
    {
        var user = (SocketGuildUser)Context.User;
        _logger.LogTrace("{User} is creating activity {Activity}", user.DisplayName, activity);

        // Activities are identified using their original message id
        await RespondAsync("> *Création de l'activité en cours...*");

        var response = await GetOriginalResponseAsync();
        activity.ActivityId = response.Id;

        // Add activity to db
        await _activitiesRepository.AddActivity(activity);

        // Add creator to activity
        var rolePlayer = new ActivityRolePlayer
        {
            Activity = activity,
            DiscordId = user.Id,
            Name = user.DisplayName,
            Roles = _activityHelper.GetPlayerRoles(user.Roles)
        };

        activity.ActivityPlayers.Add(rolePlayer);
        await _activitiesRepository.SaveChanges();

        // Add components
        var components = ActivityRoleComponent(activity.ActivityId);

        await ModifyOriginalResponseAsync(m =>
        {
            m.Content = "";
            m.Components = components.Build();
            m.Embed = _activityFormatter.ActivityEmbed(activity, Enumerable.Repeat(rolePlayer, 1).ToImmutableList()).Build();
        });
    }

    [ComponentInteraction("activity join_role:*", ignoreGroupNames: true)]
    private async Task JoinActivityRole(ulong activityId)
    {
        var user = (SocketGuildUser)Context.User;

        // Check if activity exists
        if (await _activitiesRepository.FindActivity(activityId) is not { } activity)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("Cette activité n'existe plus").Build()
            );

            return;
        }

        // If player is already registered
        if (await _activitiesRepository.FindActivityRolePlayer(activityId, user.Id) is not null)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("Vous êtes déjà inscrit à cette activité").Build()
            );

            return;
        }

        // Check if activity is full
        if (await _activitiesRepository.ActivityPlayerCount(activity) >= activity.MaxPlayers)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("L'activité est complète").Build()
            );

            return;
        }

        _logger.LogTrace("Player {Player} joined activity {Id}", user.DisplayName, activityId);

        var roles = _activityHelper.GetPlayerRoles(user.Roles);
        var activityRolePlayer = new ActivityRolePlayer
        {
            Activity = activity,
            DiscordId = user.Id,
            Name = user.DisplayName,
            Roles = roles
        };

        // Add player to activity
        activity.ActivityPlayers.Add(activityRolePlayer);
        await _activitiesRepository.SaveChanges();

        // Update activity embed
        await UpdateActivityEmbed(activity);

        await RespondAsync(
            ephemeral: true,
            embed: EmbedUtils.SuccessEmbed("Vous avez bien été inscrit pour cette activité").Build()
        );
    }

    [ComponentInteraction("activity leave_role:*", ignoreGroupNames: true)]
    private async Task LeaveActivityRole(ulong activityId)
    {
        var user = (IGuildUser)Context.User;

        // Check if activity exists
        if (await _activitiesRepository.FindActivity(activityId) is not { } activity)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("Cette activité n'existe plus").Build()
            );

            return;
        }

        // Check if player is in activity
        if (await _activitiesRepository.FindActivityPlayer(activityId, user.Id) is not { } activityPlayer)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("Vous n'êtes pas inscrit à cette activité").Build()
            );

            return;
        }

        _logger.LogTrace("Player {Player} left activity {Id}", user.DisplayName, activityId);

        activity.ActivityPlayers.Remove(activityPlayer);
        await _activitiesRepository.SaveChanges();

        // Update activity embed
        await UpdateActivityEmbed(activity);

        await RespondAsync(
            ephemeral: true,
            embed: EmbedUtils.SuccessEmbed("Vous avez bien été désinscrit pour cette activité").Build()
        );
    }

    // [ComponentInteraction("activity event_join:*", ignoreGroupNames: true)]
    // private async Task JoinEventActivity(ulong activityId)
    // {
    //     _logger.LogTrace("Player {Player} joined activity {Id}", ((IGuildUser)Context.User).DisplayName, activityId);
    //
    //     await RespondAsync(activityId.ToString());
    // }

    private async Task UpdateActivityEmbed(Activity activity)
    {
        // Get channel
        var channel = await Context.Interaction.GetChannelAsync();

        if (channel is null)
        {
            return;
        }

        // Fetch players
        var players = await _activitiesRepository.LoadActivityPlayers(activity);

        await channel.ModifyMessageAsync(activity.ActivityId, properties =>
        {
            properties.Embed = _activityFormatter.ActivityEmbed(activity, players).Build();
        });
    }

    private static ComponentBuilder ActivityRoleComponent(ulong activityId)
    {
        return new ComponentBuilder()
            .AddRow(new ActionRowBuilder()
                .WithButton(new ButtonBuilder()
                    .WithLabel("Rejoindre l'activité")
                    .WithCustomId($"activity join_role:{activityId}")
                    .WithStyle(ButtonStyle.Primary)
                )
                .WithButton(new ButtonBuilder()
                    .WithLabel("Se désinscrire de l'activité")
                    .WithCustomId($"activity leave_role:{activityId}")
                    .WithStyle(ButtonStyle.Danger)
                )
            );
    }
}
