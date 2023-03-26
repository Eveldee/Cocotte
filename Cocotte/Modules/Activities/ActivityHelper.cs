using Cocotte.Modules.Activities.Models;
using Cocotte.Options;
using Cocotte.Utils;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Cocotte.Modules.Activities;

public class ActivityHelper
{
    private const uint UnlimitedPlayers = uint.MaxValue;

    private readonly ActivityOptions _options;
    private readonly ActivitiesRepository _activitiesRepository;
    private readonly ActivityFormatter _activityFormatter;

    public ActivityHelper(IOptions<ActivityOptions> options, ActivitiesRepository activitiesRepository, ActivityFormatter activityFormatter)
    {
        _activitiesRepository = activitiesRepository;
        _activityFormatter = activityFormatter;
        _options = options.Value;
    }

    public PlayerRoles GetPlayerRoles(IEnumerable<SocketRole> userRoles)
    {
        var roles = PlayerRoles.None;

        foreach (var socketRole in userRoles)
        {
            roles |= socketRole.Id switch
            {
                var role when role == _options.HelperRoleId => PlayerRoles.Helper,
                var role when role == _options.DpsRoleId => PlayerRoles.Dps,
                var role when role == _options.TankRoleId => PlayerRoles.Tank,
                var role when role == _options.SupportRoleId => PlayerRoles.Support,
                _ => PlayerRoles.None
            };
        }

        return roles;
    }

    public static ActivityType ActivityNameToType(ActivityName activityName) => activityName switch
    {
        ActivityName.Abyss or
            ActivityName.FrontierClash or
            ActivityName.InterstellarExploration or
            ActivityName.JointOperation or
            ActivityName.VoidRift or
            ActivityName.OriginsOfWar => ActivityType.Pve4Players,

        ActivityName.Raids => ActivityType.Pve8Players,

        ActivityName.CriticalAbyss => ActivityType.Pvp8Players,

        ActivityName.BreakFromDestiny => ActivityType.Pvp3Players,

        ActivityName.Event or
            ActivityName.Fishing => ActivityType.Event8Players,

        ActivityName.MirroriaRace => ActivityType.Event4Players,

        _ => ActivityType.Other
    };

    public static uint ActivityTypeToMaxPlayers(ActivityType activityType) => activityType switch
    {
        ActivityType.Pve4Players or
        ActivityType.Event4Players => 4,

        ActivityType.Pve8Players or
        ActivityType.Pvp8Players or
        ActivityType.Event8Players => 8,

        ActivityType.Pvp3Players => 3,

        ActivityType.Other => UnlimitedPlayers,

        _ => 0
    };

    public async Task UpdateActivityEmbed(IMessageChannel channel, Activity activity, ActivityUpdateReason updateReason)
    {
        // Fetch players
        var players = await _activitiesRepository.LoadActivityPlayers(activity);

        await channel.ModifyMessageAsync(activity.MessageId, properties =>
        {
            properties.Embed = _activityFormatter.ActivityEmbed(activity, players).Build();

            // Disable join button if the activity is full on join, enable it on leave if activity is not full anymore
            var isActivityFull = players.Count >= activity.MaxPlayers;
            properties.Components = updateReason switch
            {
                ActivityUpdateReason.PlayerJoin when isActivityFull => ActivityComponents(activity.MessageId, disabled: true).Build(),
                ActivityUpdateReason.PlayerLeave when !isActivityFull => ActivityComponents(activity.MessageId, disabled: false).Build(),
                _ => Optional<MessageComponent>.Unspecified
            };
        });
    }

    public ComponentBuilder ActivityComponents(ulong activityId, bool disabled = false)
    {
        return new ComponentBuilder()
            .AddRow(new ActionRowBuilder()
                .WithButton(new ButtonBuilder()
                    .WithLabel("Rejoindre")
                    .WithCustomId($"activity join:{activityId}")
                    .WithEmote(":white_check_mark:".ToEmote())
                    .WithStyle(ButtonStyle.Primary)
                    .WithDisabled(disabled)
                )
                .WithButton(new ButtonBuilder()
                    .WithLabel("Se désinscrire")
                    .WithCustomId($"activity leave:{activityId}")
                    .WithEmote(":x:".ToEmote())
                    .WithStyle(ButtonStyle.Secondary)
                )
                .WithButton(new ButtonBuilder()
                    .WithLabel("Supprimer")
                    .WithCustomId($"activity delete:{activityId}")
                    .WithEmote(":wastebasket:".ToEmote())
                    .WithStyle(ButtonStyle.Danger)
                )
            );
    }
}