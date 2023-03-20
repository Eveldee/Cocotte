using System.Diagnostics.CodeAnalysis;
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

    public ActivityModule(ILogger<ActivityModule> logger, IOptions<ActivityOptions> options, ActivityHelper activityHelper)
    {
        _logger = logger;
        _activityHelper = activityHelper;
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
    public async Task GroupAbyss([Summary("étage", "A quel étage êtes vous")] uint stage, [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        const ActivityName activityName = ActivityName.Abyss;
        var activityType = ActivityHelper.ActivityNameToType(activityName);
        var maxPlayers = ActivityHelper.ActivityTypeToMaxPlayers(activityType);
        var activityId = Context.Interaction.Id;

        var activity = new StagedActivity(Context.User.Id, description, activityType, activityName, maxPlayers, stage);

        await CreateActivity(activity);
    }

    private async Task CreateActivity(Activity activity)
    {
        _logger.LogTrace("Creating activity {Activity}", activity);

        // Activities are identified using their original message id
        await RespondAsync("`Création de l'activité en cours...`");

        var response = await GetOriginalResponseAsync();
        var activityId = response.Id;

        // Add components
        var components = ActivityComponent(activityId);

        await ModifyOriginalResponseAsync(m =>
        {
            m.Content = "";
            m.Components = components.Build();
            // m.Embed = embed.Build();
        });
    }

    [ComponentInteraction("activity join:*", ignoreGroupNames: true)]
    private async Task JoinActivity(ulong activityId)
    {
        var user = (SocketGuildUser)Context.User;

        _logger.LogTrace("Player {Player} joined activity {Id}", user.DisplayName, activityId);

        var roles = _activityHelper.GetPlayerRoles(user.Roles);
        var activityPlayer = new ActivityRolePlayer(user.Id, user.DisplayName, roles);

        await RespondAsync(
            ephemeral: true,
            embed: EmbedUtils.SuccessEmbed("Vous avez bien été inscrit pour cette activité").Build()
        );
    }

    [ComponentInteraction("activity leave:*", ignoreGroupNames: true)]
    private async Task LeaveActivity(ulong activityId)
    {
        var user = (SocketGuildUser)Context.User;

        _logger.LogTrace("Player {Player} left activity {Id}", user.DisplayName, activityId);

        // TODO: remove the user from the activity

        await RespondAsync(
            ephemeral: true,
            embed: EmbedUtils.SuccessEmbed("Vous avez bien été désinscrit pour cette activité").Build()
        );
    }

    [ComponentInteraction("activity event_join:*", ignoreGroupNames: true)]
    private async Task JoinEventActivity(ulong activityId)
    {
        _logger.LogTrace("Player {Player} joined activity {Id}", ((SocketGuildUser)Context.User).DisplayName, activityId);

        await RespondAsync(activityId.ToString());
    }

    private static ComponentBuilder ActivityComponent(ulong activityId)
    {
        return new ComponentBuilder()
            .AddRow(new ActionRowBuilder()
                .WithButton(new ButtonBuilder()
                    .WithLabel("Rejoindre l'activité")
                    .WithCustomId($"activity join:{activityId}")
                    .WithStyle(ButtonStyle.Primary)
                )
                .WithButton(new ButtonBuilder()
                    .WithLabel("Se désinscrire de l'activité")
                    .WithCustomId($"activity leave:{activityId}")
                    .WithStyle(ButtonStyle.Danger)
                )
            );
    }
}
