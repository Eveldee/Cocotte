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
            ActivityName.OriginsOfWar => "Origine de la Guerre",
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

    public EmbedBuilder ActivityEmbed(Activity activity, IReadOnlyCollection<ActivityPlayer> players)
    {
        // Players field
        var playersField = new EmbedFieldBuilder()
            .WithName("Joueurs inscrits")
            .WithValue($"{(!players.Any() ? "*Aucun joueur inscrit*" :  string.Join("\n", players.Select(FormatActivityPlayer)))}");

        var title = activity switch
        {
            StagedActivity stagedActivity =>
                $"{FormatActivityName(activity.Name)} ({players.Count}/{activity.MaxPlayers}) - Étage {stagedActivity.Stage}",
            _ => $"{FormatActivityName(activity.Name)} ({players.Count}/{activity.MaxPlayers})"
        };

        string bannerUrl = $"https://sage.cdn.ilysix.fr/assets/Cocotte/banner/{GetActivityCode(activity.Name)}.webp";

        return new EmbedBuilder()
            .WithColor(Colors.CocotteBlue)
            .WithTitle(title)
            .WithDescription($"{activity.Description}")
            .WithImageUrl(bannerUrl)
            .WithFields(playersField);
    }

    private static string GetActivityCode(ActivityName activityName) => activityName switch
    {
        ActivityName.Abyss => "VA",
        ActivityName.OriginsOfWar => "OOW",
        ActivityName.Raids => "RD",
        ActivityName.FrontierClash => "FC",
        ActivityName.VoidRift => "VR",
        ActivityName.JointOperation => "JO",
        ActivityName.InterstellarExploration => "IE",
        ActivityName.BreakFromDestiny => "BR",
        ActivityName.CriticalAbyss => "CA",
        ActivityName.Fishing => "FI",
        ActivityName.Event => "EV",
        ActivityName.MirroriaRace => "MR",
        _ => "NA"
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