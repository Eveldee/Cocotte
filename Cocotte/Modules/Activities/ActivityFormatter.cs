using System.Text;
using Cocotte.Modules.Activities.Models;
using Cocotte.Options;
using Cocotte.Utils;
using Discord;
using Microsoft.Extensions.Options;

namespace Cocotte.Modules.Activities;

public class ActivityFormatter
{
    private readonly ActivityOptions _options;

    public ActivityFormatter(IOptions<ActivityOptions> options)
    {
        _options = options.Value;
    }

    public static string FormatActivityName(ActivityName activityName)
    {
        return activityName switch
        {
            ActivityName.Abyss => "Abîme du Néant",
            ActivityName.Raids => "Raids",
            ActivityName.FrontierClash => "Conflit Frontalier",
            ActivityName.VoidRift => "Failles du Néant",
            ActivityName.OriginsOfWar => "Origines de la Guerre",
            ActivityName.JointOperation => "Opération Conjointe",
            ActivityName.InterstellarExploration => "Exploration Interstellaire",
            ActivityName.BreakFromDestiny => "Échapper au Destin (3v3)",
            ActivityName.CriticalAbyss => "Abîme Critique (8v8)",
            ActivityName.Event => "Event",
            ActivityName.Fishing => "Pêche",
            ActivityName.MirroriaRace => "Course Mirroria",
            _ => throw new ArgumentOutOfRangeException(nameof(activityName), activityName, null)
        };
    }

    public EmbedBuilder ActivityEmbed(Activity activity, IEnumerable<ActivityPlayer> players)
    {
        // Get activity emote
        var activityEmote = GetActivityEmote(activity.Name);

        // Players field
        var playersField = new EmbedFieldBuilder()
            .WithName("Joueurs inscrits")
            .WithValue($"{(!players.Any() ? "*Aucun joueur inscrit*" :  string.Join("\n", players.Select(FormatActivityPlayer)))}");

        return new EmbedBuilder()
            .WithColor(Colors.CocotteBlue)
            .WithTitle($"{activityEmote} {FormatActivityName(activity.Name)} ({players.Count()}/{activity.MaxPlayers})")
            .WithDescription($"{activity.Description}")
            .WithFields(playersField);
    }

    private static string GetActivityEmote(ActivityName activityName) => activityName switch
    {
        ActivityName.Abyss => ":fox:",
        ActivityName.Raids => ":crossed_swords:",
        ActivityName.Fishing => ":fishing_pole_and_fish:",
        _ => ":white_circle:"
    };

    public string FormatActivityPlayer(ActivityPlayer player) => player switch
    {
        ActivityRolePlayer rolePlayer => $"{player.Name} - {RolesToEmotes(rolePlayer.Roles)}",
        _ => $"{player.Name})"
    };

    private string RolesToEmotes(ActivityRoles rolePlayerRoles)
    {
        var emotesBuilder = new StringBuilder();

        if (rolePlayerRoles.HasFlag(ActivityRoles.Helper))
        {
            emotesBuilder.Append($" {_options.HelperEmote} ");
        }

        if (rolePlayerRoles.HasFlag(ActivityRoles.Dps))
        {
            emotesBuilder.Append($" {_options.DpsEmote} ");
        }

        if (rolePlayerRoles.HasFlag(ActivityRoles.Tank))
        {
            emotesBuilder.Append($" {_options.TankEmote} ");
        }

        if (rolePlayerRoles.HasFlag(ActivityRoles.Support))
        {
            emotesBuilder.Append($" {_options.SupportEmote} ");
        }

        return emotesBuilder.ToString();
    }
}