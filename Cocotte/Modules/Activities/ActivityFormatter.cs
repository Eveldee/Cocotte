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
    private readonly InterstellarFormatter _interstellarFormatter;

    public ActivityFormatter(IOptions<ActivityOptions> options, InterstellarFormatter interstellarFormatter)
    {
        _interstellarFormatter = interstellarFormatter;
        _options = options.Value;
    }

    public string FormatActivityName(ActivityName activityName)
    {
        return activityName switch
        {
            ActivityName.Abyss => "Abîme du Néant",
            ActivityName.Raids => "Raids",
            ActivityName.FrontierClash => "Conflit frontalier",
            ActivityName.VoidRift => "Failles du néant",
            ActivityName.OriginsOfWar => "Origine de la guerre",
            ActivityName.JointOperation => "Opération conjointe",
            ActivityName.InterstellarExploration => "Porte interstellaire",
            ActivityName.BreakFromDestiny => "Échapper au destin (3v3)",
            ActivityName.CriticalAbyss => "Abîme critique (8v8)",
            ActivityName.Event => "Event",
            ActivityName.Fishing => "Pêche",
            ActivityName.MirroriaRace => "Course Mirroria",
            _ => throw new ArgumentOutOfRangeException(nameof(activityName), activityName, null)
        };
    }

    public string GetActivityBanner(ActivityName activityName)
    {
        return CdnUtils.GetAsset($"banner/{GetActivityCode(activityName)}.webp");
    }

    public EmbedBuilder ActivityEmbed(Activity activity, IReadOnlyCollection<ActivityPlayer> players)
    {
        // Activity full
        bool activityFull = players.Count >= activity.MaxPlayers;

        // Compute padding using player with longest name
        var namePadding = players.Count > 0 ? players.Max(p => p.Name.Length) : 0;

        // Players field
        var playersField = new EmbedFieldBuilder()
            .WithName("Joueurs inscrits")
            .WithValue($"{(!players.Any() ? "*Aucun joueur inscrit*" :  string.Join("\n", players.Select(p => FormatActivityPlayer(p, namePadding))))}");

        var title = activity switch
        {
            StagedActivity stagedActivity =>
                $"{FormatActivityName(activity.Name)} ({players.Count}/{activity.MaxPlayers}) - Étage {stagedActivity.Stage}",
            InterstellarActivity interstellar =>
                $"{FormatActivityName(activity.Name)} {_interstellarFormatter.FormatInterstellarColor(interstellar.Color)} ({players.Count}/{activity.MaxPlayers})",
            _ =>
                $"{FormatActivityName(activity.Name)} ({players.Count}/{activity.MaxPlayers})"
        };

        // Build description
        var descriptionBuilder = new StringBuilder();

        // Add time if specified
        if (activity.DueTime is { } time)
        {
            descriptionBuilder.AppendLine($"**:clock2: {TimestampTag.FormatFromDateTime(DateTime.Today.WithTimeOnly(time), TimestampTagStyles.ShortTime)}**");
        }
        else
        {
            descriptionBuilder.AppendLine($"**:clock2: Maintenant**");
        }

        // Add generic message or specified activity description
        descriptionBuilder.AppendLine(
            string.IsNullOrWhiteSpace(activity.Description)
                ? $"Rejoignez l'activité de {MentionUtils.MentionUser(activity.CreatorUserId)}"
                : activity.Description
        );

        // Add thread link
        descriptionBuilder.AppendLine();
        descriptionBuilder.Append($"**[Fil associé]({ChannelUtils.GetChannelLink(activity.GuildId, activity.ThreadId)})**");

        string bannerUrl = GetActivityBanner(activity.Name);

        var color = activityFull ? Colors.CocotteOrange : Colors.CocotteBlue;

        var builder = new EmbedBuilder()
            .WithColor(color)
            .WithTitle(title)
            .WithDescription(descriptionBuilder.ToString())
            .WithImageUrl(bannerUrl)
            .WithFields(playersField);

        // Add material for interstellar exploration
        if (activity is InterstellarActivity interstellarActivity)
        {
            builder.WithThumbnailUrl(_interstellarFormatter.GetColorIcon(interstellarActivity.Color));
        }

        return builder;
    }

    private string GetActivityCode(ActivityName activityName) => activityName switch
    {
        ActivityName.Abyss => "VA",
        ActivityName.OriginsOfWar => "OOW",
        ActivityName.Raids => "RD",
        ActivityName.FrontierClash => "FCH",
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

    public string FormatActivityPlayer(ActivityPlayer player, int namePadding) => player switch
    {
        ActivityRolePlayer rolePlayer => $"` {player.Name.PadRight(namePadding)} ` **―** {RolesToEmotes(rolePlayer.Roles)}",
        _ => $"` {player.Name} `"
    };

    private string RolesToEmotes(PlayerRoles rolePlayerRoles)
    {
        var emotesBuilder = new StringBuilder();

        if (rolePlayerRoles.HasFlag(PlayerRoles.Helper))
        {
            emotesBuilder.Append($" {_options.HelperEmote} ");
        }

        if (rolePlayerRoles.HasFlag(PlayerRoles.Dps))
        {
            emotesBuilder.Append($" {_options.DpsEmote} ");
        }

        if (rolePlayerRoles.HasFlag(PlayerRoles.Tank))
        {
            emotesBuilder.Append($" {_options.TankEmote} ");
        }

        if (rolePlayerRoles.HasFlag(PlayerRoles.Support))
        {
            emotesBuilder.Append($" {_options.SupportEmote} ");
        }

        return emotesBuilder.ToString();
    }
}