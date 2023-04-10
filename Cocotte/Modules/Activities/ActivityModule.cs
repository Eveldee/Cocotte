using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Cocotte.Modules.Activities.Models;
using Cocotte.Options;
using Cocotte.Utils;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Quartz;
using Alias = Discord.Commands.AliasAttribute;

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
    private readonly ISchedulerFactory _schedulerFactory;

    private SocketGuildUser User => (SocketGuildUser) Context.User;

    public ActivityModule(ILogger<ActivityModule> logger, IOptions<ActivityOptions> options,
        ActivityHelper activityHelper, ActivitiesRepository activitiesRepository,
        ActivityFormatter activityFormatter, ISchedulerFactory schedulerFactory)
    {
        _logger = logger;
        _activityHelper = activityHelper;
        _activitiesRepository = activitiesRepository;
        _activityFormatter = activityFormatter;
        _schedulerFactory = schedulerFactory;
        _options = options.Value;
    }

#if DEBUG
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
#endif

    #region Activities

    [SlashCommand("abime-néant", "Créer un groupe pour l'Abîme du Néant")]
    [Alias("abime", "abyss")]
    public async Task ActivityVoidAbyss(
        [Summary("étage", "A quel étage vous êtes")] [MinValue(1), MaxValue(6)] uint stage,
        [Summary("heure", "Heure à laquelle l'activité est prévue")] string? timeInput = null,
        [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        await CreateActivity(ActivityName.Abyss, timeInput, description, stage: stage);
    }

    [SlashCommand("origine-guerre", "Créer un groupe pour l'Origine de la guerre")]
    [Alias("origine", "OOW")]
    public async Task ActivityOrigins(
        [Summary("étage", "A quel étage vous êtes")] [MinValue(1), MaxValue(25)] uint stage,
        [Summary("heure", "Heure à laquelle l'activité est prévue")] string? timeInput = null,
        [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        await CreateActivity(ActivityName.OriginsOfWar, timeInput, description, stage: stage);
    }

    [SlashCommand("raids", "Créer un groupe pour les Raids")]
    public async Task ActivityRaids(
        [Summary("heure", "Heure à laquelle l'activité est prévue")] string? timeInput = null,
        [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        await CreateActivity(ActivityName.Raids, timeInput, description);
    }

    [SlashCommand("conflit-frontalier", "Créer un groupe pour Conflit frontalier")]
    [Alias("conflit", "FC")]
    public async Task ActivityFrontierClash(
        [Summary("heure", "Heure à laquelle l'activité est prévue")] string? timeInput = null,
        [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        await CreateActivity(ActivityName.FrontierClash, timeInput, description);
    }

    [SlashCommand("failles-neant", "Créer un groupe pour les Failles du néant")]
    [Alias("failles", "rift")]
    public async Task ActivityVoidRift(
        [Summary("heure", "Heure à laquelle l'activité est prévue")] string? timeInput = null,
        [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        await CreateActivity(ActivityName.VoidRift, timeInput, description);
    }

    [SlashCommand("operations-conjointes", "Créer un groupe pour les Opérations conjointes")]
    [Alias("operations", "JO")]
    public async Task ActivityJointOperation(
        [Summary("heure", "Heure à laquelle l'activité est prévue")] string? timeInput = null,
        [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        await CreateActivity(ActivityName.JointOperation, timeInput, description);
    }

    [SlashCommand("portes-interstellaires", "Créer un groupe pour les Portes interstellaires")]
    [Alias("portes")]
    public async Task ActivityInterstellarExploration(
        [Summary("couleur", "De quel couleur de matériaux s'agît-il")] InterstellarColor color,
        [Summary("heure", "Heure à laquelle l'activité est prévue")] string? timeInput = null,
        [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        await CreateActivity(ActivityName.InterstellarExploration, timeInput, description, areRolesEnabled: false,
            interstellarColor: color);
    }

    [SlashCommand("3v3", "Créer un groupe pour le 3v3 (Échapper au destin)")]
    [Alias("BR")]
    public async Task ActivityBreakFromDestiny(
        [Summary("heure", "Heure à laquelle l'activité est prévue")] string? timeInput = null,
        [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        await CreateActivity(ActivityName.BreakFromDestiny, timeInput, description, areRolesEnabled: false);
    }

    [SlashCommand("8v8", "Créer un groupe pour le 8v8 (Abîme critique)")]
    [Alias("critical")]
    public async Task ActivityCriticalAbyss(
        [Summary("heure", "Heure à laquelle l'activité est prévue")] string? timeInput = null,
        [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        await CreateActivity(ActivityName.CriticalAbyss, timeInput, description);
    }

    [SlashCommand("evenement", "Créer un groupe pour les évènements")]
    [Alias("event")]
    public async Task ActivityMinigame(
        [Summary("joueurs", "Nombre de joueurs maximum pour cette activité")] [MinValue(2), MaxValue(16)]
        uint maxPlayers = 8, [Summary("heure", "Heure à laquelle l'activité est prévue")] string? timeInput = null,
        [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        await CreateActivity(ActivityName.Minigame, timeInput, description, areRolesEnabled: false,
            maxPlayers: maxPlayers);
    }

    [SlashCommand("peche", "Créer un groupe pour de la pêche")]
    [Alias("fishing")]
    public async Task ActivityFishing(
        [Summary("heure", "Heure à laquelle l'activité est prévue")] string? timeInput = null,
        [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        await CreateActivity(ActivityName.Fishing, timeInput, description, areRolesEnabled: false);
    }

    [SlashCommand("course", "Créer un groupe pour les courses de Mirroria")]
    [Alias("BR")]
    public async Task ActivityMirroriaRace(
        [Summary("heure", "Heure à laquelle l'activité est prévue")] string? timeInput = null,
        [Summary("description", "Message accompagnant la demande de groupe")] string description = "")
    {
        await CreateActivity(ActivityName.MirroriaRace, timeInput, description, areRolesEnabled: false);
    }

    #endregion

    #region Organization

    [SlashCommand("organiser-abime", "Organiser un groupe d'aide pour l'abîme du néant")]
    public async Task OrganizeVoidAbyss(
        [Summary("jour", "Jour auquel l'activité est prévu")] CocotteDayOfWeek day,
        [Summary("heure", "Heure à laquelle l'activité est prévue")] string timeInput,
        [Summary("joueurs", "Nombre de joueurs maximum qui peuvent s'inscrire")] uint maxPlayers,
        string description = "")
    {
        if (!TimeOnly.TryParse(timeInput, out var time))
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils
                    .ErrorEmbed(
                        "**Heure invalide**, essayez avec le **format** `heure:minutes`\nPar exemple: `15:30`")
                    .Build()
            );

            return;
        }

        // Calculate date
        var date = DateTimeUtils.NextDateWithDayAndTime((DayOfWeek) day, time);

        await CreateActivity(ActivityName.Abyss, null, description, true, maxPlayers, date: date);
    }

    [SlashCommand("organiser-origine", "Organiser un groupe d'aide pour l'origine de la guerre")]
    public async Task OrganizeOrigins(
        [Summary("jour", "Jour auquel l'activité est prévu")] CocotteDayOfWeek day,
        [Summary("heure", "Heure à laquelle l'activité est prévue")] string timeInput,
        [Summary("joueurs", "Nombre de joueurs maximum qui peuvent s'inscrire")] uint maxPlayers,
        string description = "")
    {
        if (!TimeOnly.TryParse(timeInput, out var time))
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils
                    .ErrorEmbed(
                        "**Heure invalide**, essayez avec le **format** `heure:minutes`\nPar exemple: `15:30`")
                    .Build()
            );

            return;
        }

        // Calculate date
        var date = DateTimeUtils.NextDateWithDayAndTime((DayOfWeek) day, time);

        await CreateActivity(ActivityName.OriginsOfWar, null, description, true, maxPlayers, date: date);
    }

    [SlashCommand("organiser-evenement", "Organiser un groupe d'aide pour l'abîme du néant")]
    public async Task OrganizeEvent(
        [Summary("jour", "Jour auquel l'activité est prévu")] CocotteDayOfWeek day,
        [Summary("heure", "Heure à laquelle l'activité est prévue")] string timeInput,
        [Summary("joueurs", "Nombre de joueurs maximum qui peuvent s'inscrire")] uint maxPlayers,
        string description = "")
    {
        if (!TimeOnly.TryParse(timeInput, out var time))
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils
                    .ErrorEmbed(
                        "**Heure invalide**, essayez avec le **format** `heure:minutes`\nPar exemple: `15:30`")
                    .Build()
            );

            return;
        }

        // Calculate date
        var date = DateTimeUtils.NextDateWithDayAndTime((DayOfWeek) day, time);

        await CreateActivity(ActivityName.Event, null, description, false, maxPlayers, date: date);
    }

    #endregion

    private async Task CreateActivity(ActivityName activityName, string? timeInput, string description,
        bool areRolesEnabled = true, uint? maxPlayers = null, uint? stage = null,
        InterstellarColor? interstellarColor = null, DateTime? date = null)
    {
        // Check time if it's specified
        var dueDate = date;
        if (timeInput is not null)
        {
            if (!TimeOnly.TryParse(timeInput, out var parsedTime))
            {
                await RespondAsync(
                    ephemeral: true,
                    embed: EmbedUtils
                        .ErrorEmbed(
                            "**Heure invalide**, essayez avec le **format** `heure:minutes`\nPar exemple: `15:30`")
                        .Build()
                );

                return;
            }

            dueDate = DateTimeUtils.NextDateWithTimeOfDay(parsedTime);
        }

        _logger.LogTrace("{User} is creating activity {Activity}", User.DisplayName, activityName);

        // Activities are identified using their original message id
        await RespondAsync("> ***Création de l'activité en cours...***");

        var response = await GetOriginalResponseAsync();

        // Create associated thread
        var threadId = await CreateThread(activityName, User.DisplayName);

        var activityType = ActivityHelper.ActivityNameToType(activityName);
        maxPlayers ??= ActivityHelper.ActivityTypeToMaxPlayers(activityType);
        Activity activity;

        // Create organized activity if date is not null
        if (date is not null)
        {
            activity = new OrganizedActivity
            {
                MessageId = response.Id,
                ChannelId = Context.Channel.Id,
                GuildId = Context.Guild.Id,
                ThreadId = threadId,
                CreatorUserId = Context.User.Id,
                CreatorDisplayName = ((SocketGuildUser) Context.User).DisplayName,
                Description = description,
                DueDateTime = dueDate,
                Type = activityType,
                Name = activityName,
                AreRolesEnabled = areRolesEnabled,
                MaxPlayers = (uint) maxPlayers
            };
        }
        else if (stage is not null)
        {
            activity = new StagedActivity
            {
                MessageId = response.Id,
                ChannelId = Context.Channel.Id,
                GuildId = Context.Guild.Id,
                ThreadId = threadId,
                CreatorUserId = Context.User.Id,
                CreatorDisplayName = ((SocketGuildUser) Context.User).DisplayName,
                Description = description,
                DueDateTime = dueDate,
                Type = activityType,
                Name = activityName,
                AreRolesEnabled = areRolesEnabled,
                MaxPlayers = (uint) maxPlayers,
                Stage = (uint) stage
            };
        }
        else if (interstellarColor is not null)
        {
            activity = new InterstellarActivity
            {
                MessageId = response.Id,
                ChannelId = Context.Channel.Id,
                GuildId = Context.Guild.Id,
                ThreadId = threadId,
                CreatorUserId = Context.User.Id,
                CreatorDisplayName = ((SocketGuildUser) Context.User).DisplayName,
                Description = description,
                DueDateTime = dueDate,
                Type = activityType,
                Name = activityName,
                AreRolesEnabled = false,
                MaxPlayers = (uint) maxPlayers,
                Color = (InterstellarColor) interstellarColor
            };
        }
        else
        {
            activity = new Activity
            {
                MessageId = response.Id,
                ChannelId = Context.Channel.Id,
                GuildId = Context.Guild.Id,
                ThreadId = threadId,
                CreatorUserId = Context.User.Id,
                CreatorDisplayName = ((SocketGuildUser) Context.User).DisplayName,
                Description = description,
                DueDateTime = dueDate,
                Type = activityType,
                Name = activityName,
                AreRolesEnabled = areRolesEnabled,
                MaxPlayers = (uint) maxPlayers
            };
        }

        // Add activity to db
        await _activitiesRepository.AddActivity(activity);

        // Add creator to activity, make it an helper
        var activityPlayer = CreateActivityPlayer(activity, User, areRolesEnabled, activity is OrganizedActivity);

        activity.ActivityPlayers.Add(activityPlayer);

        await _activitiesRepository.SaveChanges();

        // Add components
        var components = _activityHelper.ActivityComponents(activity.MessageId);

        await ModifyOriginalResponseAsync(m =>
        {
            m.Content = "";
            m.Components = components.Build();
            m.Embed = _activityFormatter.ActivityEmbed(activity, Enumerable.Repeat(activityPlayer, 1).ToImmutableList())
                .Build();
        });

        // Add job to close this activity in scheduler if due date is specified
        if (dueDate is { } dueDateTime)
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var job = JobBuilder.Create<ActivityCloseJob>()
                .WithIdentity(activity.JobKey, "activity-close")
                .WithDescription("Automatically close an activity after due date has passed")
                .UsingJobData(nameof(ActivityCloseJob.GuildId), (long) activity.GuildId)
                .UsingJobData(nameof(ActivityCloseJob.ChannelId), (long) activity.ChannelId)
                .UsingJobData(nameof(ActivityCloseJob.MessageId), (long) activity.MessageId)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{activity.JobKey}-trigger", "activity-close")
                .StartAt(dueDateTime)
                .ForJob(job)
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }

    private ActivityPlayer CreateActivityPlayer(Activity activity, SocketGuildUser user, bool areRolesEnabled, bool isOrganizer = false)
    {
        return areRolesEnabled
            ? new ActivityRolePlayer
            {
                Activity = activity,
                UserId = user.Id,
                Name = user.DisplayName,
                Roles = _activityHelper.GetPlayerRoles(user.Roles),
                IsOrganizer = isOrganizer
            }
            : new ActivityPlayer
            {
                Activity = activity,
                UserId = user.Id,
                Name = user.DisplayName,
                IsOrganizer = isOrganizer
            };
    }

    [ComponentInteraction("activity join:*", ignoreGroupNames: true)]
    public async Task JoinActivity(ulong messageId)
    {
        // Check if activity exists
        if (await _activitiesRepository.FindActivity(Context.Guild.Id, Context.Channel.Id, messageId) is not
            { } activity)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("Cette activité **n'existe plus**").Build()
            );

            return;
        }

        if (!await AddUserToActivity(activity, User, self: true))
        {
            return;
        }

        await RespondAsync(
            ephemeral: true,
            embed: EmbedUtils.SuccessEmbed("Vous avez bien été **inscrit** pour cette activité").Build()
        );
    }

    [ComponentInteraction("activity leave:*", ignoreGroupNames: true)]
    public async Task LeaveActivity(ulong messageId)
    {
        // Check if activity exists
        if (await _activitiesRepository.FindActivity(Context.Guild.Id, Context.Channel.Id, messageId) is not
            { } activity)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("Cette activité **n'existe plus**").Build()
            );

            return;
        }

        if (!await RemovePlayerFromActivity(activity, User, self: true))
        {
            return;
        }

        await RespondAsync(
            ephemeral: true,
            embed: EmbedUtils.SuccessEmbed("Vous avez bien été **désinscrit** pour cette activité").Build()
        );
    }

    [ComponentInteraction("activity delete:*", ignoreGroupNames: true)]
    public async Task DeleteActivity(ulong messageId)
    {
        // Check if activity exists
        if (await _activitiesRepository.FindActivity(Context.Guild.Id, Context.Channel.Id, messageId) is not
            { } activity)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("Cette activité **n'existe plus**").Build()
            );

            return;
        }

        // Check if user has permission to delete this activity
        if (User.Id != activity.CreatorUserId && Context.User.Id != (await Context.Client.GetApplicationInfoAsync()).Owner.Id)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("Vous n'avez **pas la permission** d'exécuter cette action").Build()
            );

            return;
        }

        // Remove from database
        _activitiesRepository.DeleteActivity(activity);
        await _activitiesRepository.SaveChanges();

        // Remove thread
        await Context.Guild.GetThreadChannel(activity.ThreadId).DeleteAsync();

        // Delete response
        await Context.Channel.DeleteMessageAsync(messageId);
    }

    private async Task<bool> AddUserToActivity(Activity activity, SocketGuildUser user, bool self)
    {
        // If player is already registered
        if (await _activitiesRepository.FindActivityPlayer(activity.GuildId, activity.ChannelId, activity.MessageId,
                user.Id) is not
            null)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils
                    .ErrorEmbed(self
                        ? "Vous êtes **déjà inscrit** à cette activité"
                        : $"{MentionUtils.MentionUser(user.Id)} est **déjà inscrit** à cette activité").Build()
            );

            return false;
        }

        // If activity is an organized activity and user is an organizer, bypass full and closed checks
        var isOrganizer = activity is OrganizedActivity organizedActivity &&
                          _activityHelper.IsOrganizer(organizedActivity, user);
        if (!isOrganizer)
        {
            // Check if activity is closed
            if (activity.IsClosed)
            {
                await RespondAsync(
                    ephemeral: true,
                    embed: EmbedUtils.ErrorEmbed("Cette activité est fermée").Build()
                );

                return false;
            }

            // Check if activity is full
            if (await _activitiesRepository.ActivityPlayerCount(activity) >= activity.MaxPlayers)
            {
                await RespondAsync(
                    ephemeral: true,
                    embed: EmbedUtils.ErrorEmbed("L'activité est **complète**").Build()
                );

                return false;
            }
        }

        _logger.LogTrace("Player {Player} joined activity {Id}", user.DisplayName, activity.MessageId);

        var activityPlayer = CreateActivityPlayer(activity, user, activity.AreRolesEnabled, isOrganizer: isOrganizer);

        // Add player to activity
        activity.ActivityPlayers.Add(activityPlayer);
        await _activitiesRepository.SaveChanges();

        // Update activity embed
        await UpdateActivityEmbed(activity, ActivityUpdateReason.PlayerJoin);

        // Send join message to thread and add user to thread
        var thread = Context.Guild.GetThreadChannel(activity.ThreadId);

        await thread.AddUserAsync(user);

        string embedContent = $"{MentionUtils.MentionUser(user.Id)} a été **ajouté** à l'activité";
        if (self)
        {
            await thread.SendMessageAsync(
                embed: EmbedUtils.InfoEmbed(embedContent).Build()
            );
        }
        else
        {
            await RespondAsync(
                embed: EmbedUtils.InfoEmbed(embedContent).Build()
            );
        }

        return true;
    }

    private async Task<bool> RemovePlayerFromActivity(Activity activity, SocketGuildUser user, bool self)
    {
        // Check if player is in activity
        if (await _activitiesRepository.FindActivityPlayer(activity.GuildId, activity.ChannelId, activity.MessageId,
                user.Id) is not { } activityPlayer)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils
                    .ErrorEmbed(self
                        ? "Vous n'êtes **pas inscrit** à cette activité"
                        : $"{MentionUtils.MentionUser(user.Id)} n'est **pas inscrit** à cette activité").Build()
            );

            return false;
        }

        _logger.LogTrace("Player {Player} left activity {Id}", user.DisplayName, activity.MessageId);

        activity.ActivityPlayers.Remove(activityPlayer);
        await _activitiesRepository.SaveChanges();

        // Update activity embed
        await UpdateActivityEmbed(activity, ActivityUpdateReason.PlayerLeave);

        // Send leave message to thread and remove user from thread
        var thread = Context.Guild.GetThreadChannel(activity.ThreadId);

        await thread.RemoveUserAsync(user);

        string embedContent = $"{MentionUtils.MentionUser(user.Id)} a été **enlevé** de l'activité";
        if (self)
        {
            await thread.SendMessageAsync(
                embed: EmbedUtils.InfoEmbed(embedContent).Build()
            );
        }
        else
        {
            await RespondAsync(
                embed: EmbedUtils.InfoEmbed(embedContent).Build()
            );
        }

        return true;
    }

    private async Task UpdateActivityEmbed(Activity activity, ActivityUpdateReason updateReason)
    {
        // Get channel
        if (Context.Guild.GetChannel(activity.ChannelId) is not SocketTextChannel channel)
        {
            return;
        }

        await _activityHelper.UpdateActivityEmbed(channel, activity, updateReason);
    }
}