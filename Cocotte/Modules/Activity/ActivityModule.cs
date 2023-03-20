using System.Diagnostics.CodeAnalysis;
using Cocotte.Modules.Activity.Models;
using Cocotte.Options;
using Cocotte.Utils;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Cocotte.Modules.Activity;

/// <summary>
/// Module to ask and propose groups for different activities: Abyss, OOW, FC, ...
/// </summary>
[Group("activite", "Organise des activités")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class ActivityModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<ActivityModule> _logger;
    private readonly ActivityOptions _options;
    private readonly ActivityHelper _activityHelper;
    private readonly ActivitiesRepository _activitiesRepository;

    public ActivityModule(ILogger<ActivityModule> logger, IOptions<ActivityOptions> options, ActivityHelper activityHelper, ActivitiesRepository activitiesRepository)
    {
        _logger = logger;
        _activityHelper = activityHelper;
        _activitiesRepository = activitiesRepository;
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

    [SlashCommand("abyss", "Créer un groupe pour l'Abîme du Néant")]
    public async Task ActivityAbyss([Summary("étage", "A quel étage êtes vous")] uint stage, [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        const ActivityName activityName = ActivityName.Abyss;
        var activityType = ActivityHelper.ActivityNameToType(activityName);
        var maxPlayers = ActivityHelper.ActivityTypeToMaxPlayers(activityType);

        var activity = new StagedActivity
        {
            ActivityId = 0,
            CreatorId = Context.User.Id,
            Description = description,
            ActivityType = activityType,
            ActivityName = activityName,
            MaxPlayers = maxPlayers,
            Stage = stage,
            ActivityPlayers = new()
        };

        await CreateRoleActivity(activity);
    }

    private async Task CreateRoleActivity(Models.Activity activity)
    {
        _logger.LogTrace("Creating activity {Activity}", activity);

        // Activities are identified using their original message id
        await RespondAsync("`Création de l'activité en cours...`");

        var response = await GetOriginalResponseAsync();
        activity.ActivityId = response.Id;

        // Add activity to db
        await _activitiesRepository.AddActivity(activity);

        // Add components
        var components = ActivityRoleComponent(activity.ActivityId);

        await ModifyOriginalResponseAsync(m =>
        {
            m.Content = "";
            m.Components = components.Build();
            // m.Embed = embed.Build();
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

        _logger.LogTrace("Player {Player} joined activity {Id}", user.DisplayName, activityId);

        var roles = _activityHelper.GetPlayerRoles(user.Roles);
        var activityRolePlayer = new ActivityRolePlayer { UserId = user.Id, PlayerName = user.DisplayName, Roles = roles, ActivityId = activityId, Activity = activity };

        // Add player to activity
        activity.ActivityPlayers.Add(activityRolePlayer);
        await _activitiesRepository.SaveChanges();

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

        await RespondAsync(
            ephemeral: true,
            embed: EmbedUtils.SuccessEmbed("Vous avez bien été désinscrit pour cette activité").Build()
        );
    }

    [ComponentInteraction("activity event_join:*", ignoreGroupNames: true)]
    private async Task JoinEventActivity(ulong activityId)
    {
        _logger.LogTrace("Player {Player} joined activity {Id}", ((IGuildUser)Context.User).DisplayName, activityId);

        await RespondAsync(activityId.ToString());
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
