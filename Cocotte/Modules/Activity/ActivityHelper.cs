using Cocotte.Options;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Cocotte.Modules.Activity;

public class ActivityHelper
{
    private const uint UnlimitedPlayers = uint.MaxValue;

    private readonly ActivityOptions _options;

    public ActivityHelper(IOptions<ActivityOptions> options)
    {
        _options = options.Value;
    }

    public ActivityRoles GetPlayerRoles(IReadOnlyCollection<SocketRole> userRoles)
    {
        var roles = ActivityRoles.None;

        foreach (var socketRole in userRoles)
        {
            if (socketRole.Id == _options.HelperRoleId)
            {
                roles |= ActivityRoles.Helper;
            }
            else if (socketRole.Id == _options.DpsRoleId)
            {
                roles |= ActivityRoles.Dps;
            }
            else if (socketRole.Id == _options.TankRoleId)
            {
                roles |= ActivityRoles.Tank;
            }
            else if (socketRole.Id == _options.SupportRoleId)
            {
                roles |= ActivityRoles.Support;
            }
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
        ActivityType.Event8Players => 8,

        ActivityType.Pvp3Players => 3,

        ActivityType.Other => UnlimitedPlayers,

        _ => 0
    };
}