using System.Text;
using Cocotte.Modules.Activities.Models;
using Cocotte.Options;
using Cocotte.Utils;
using Discord;
using Microsoft.Extensions.Options;

namespace Cocotte.Modules.Activities;

public class ActivityFormatter
{
    private const int FieldsChunkSize = 20;
    private const string EmptyField = "\u200B";

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
            ActivityName.Minigame => "Évènement",
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
        int GetNamesPadding(IReadOnlyCollection<ActivityPlayer> activityPlayers) => activityPlayers.Count > 0 ? activityPlayers.Max(p => p.Name.Length) : 0;

        // Load activity players and organizers
        var participants = activity.Participants.OrderBy(p => p.HasCompleted).ToArray();
        var organizers = activity.Organizers.ToArray();

        // Activity full
        bool activityFull = participants.Length >= activity.MaxPlayers;

        // Players and organizers fields
        var fields = new List<EmbedFieldBuilder>();

        // Add organizers if it's an organized activity
        if (activity is OrganizedActivity)
        {
            var organizersFields = organizers.Chunk(FieldsChunkSize).Select(organizersChunk =>
                    new EmbedFieldBuilder()
                        .WithName(EmptyField)
                        .WithIsInline(true)
                        .WithValue($"{(!organizersChunk.Any() ? "*Aucun organisateur inscrit*" :  string.Join("\n", organizersChunk.Select(p => FormatActivityPlayer(p, GetNamesPadding(organizersChunk), showOrganizerRole: ActivityHelper.IsEventActivity(activity.Type)))))}")
            ).ToArray();

            if (organizersFields.Length > 0)
            {
                organizersFields[0].Name = "Organisateurs";
            }

            // Complete with empty fields to go to next line
            var emptyFields = Enumerable.Repeat(0, (3 - organizersFields.Length) % 3).Select(_ =>
                new EmbedFieldBuilder()
                    .WithName(EmptyField)
                    .WithValue(EmptyField)
                    .WithIsInline(true)
            );

            fields.AddRange(organizersFields);
            fields.AddRange(emptyFields);
        }

        // Players field
        var playersFields = participants.Chunk(FieldsChunkSize).Select(participantsChunk =>
                new EmbedFieldBuilder()
                    .WithName(EmptyField)
                    .WithIsInline(true)
                    .WithValue($"{(!participantsChunk.Any() ? "*Aucun joueur inscrit*" :  string.Join("\n", participantsChunk.Select(p => FormatActivityPlayer(p, GetNamesPadding(participantsChunk), hideRoles: activity is OrganizedActivity))))}")
        ).ToList();

        // Insert empty fields in third column
        for (int i = 2; i <= playersFields.Count; i += 3)
        {
            playersFields.Insert(i, new EmbedFieldBuilder().WithName(EmptyField).WithValue(EmptyField).WithIsInline(true));
        }

        if (playersFields.Count > 0)
        {
            playersFields[0].Name = "Joueurs inscrits";
        }

        fields.AddRange(playersFields);

        string countTitlePart = activity.MaxPlayers == ActivityHelper.UnlimitedPlayers
            ? ""
            : $"({participants.Length}/{activity.MaxPlayers})";
        var title = activity switch
        {
            StagedActivity stagedActivity =>
                $"{FormatActivityName(activity.Name)} {countTitlePart} - Étage {stagedActivity.Stage}",
            InterstellarActivity interstellar =>
                $"{FormatActivityName(activity.Name)} {_interstellarFormatter.FormatInterstellarColor(interstellar.Color)} {countTitlePart}",
            OrganizedActivity =>
                $"{(ActivityHelper.IsEventActivity(activity.Type) ? "Organisation d'évènement" : $"Proposition d'aide - {FormatActivityName(activity.Name)}")} {countTitlePart}",
            _ =>
                $"{FormatActivityName(activity.Name)} {countTitlePart}"
        };

        // Build description
        var descriptionBuilder = new StringBuilder();

        // Add time if specified
        if (activity.DueDateTime is { } dueDateTime)
        {
            // Also Add date of organized
            if (activity is OrganizedActivity)
            {
                descriptionBuilder.AppendLine($"**:date: {TimestampTag.FormatFromDateTime(dueDateTime, TimestampTagStyles.LongDate)}**");
            }
            descriptionBuilder.AppendLine($"**:clock2: {TimestampTag.FormatFromDateTime(dueDateTime, TimestampTagStyles.ShortTime)} | {(activity.IsClosed ? "Fermée" : TimestampTag.FormatFromDateTime(dueDateTime, TimestampTagStyles.Relative))}**");
            descriptionBuilder.AppendLine();
        }
        else
        {
            descriptionBuilder.AppendLine("**:clock2: Maintenant**");
        }

        // Add generic message or specified activity description
        descriptionBuilder.AppendLine(
            string.IsNullOrWhiteSpace(activity.Description)
                ? $"Rejoignez l'activité de {MentionUtils.MentionUser(activity.CreatorUserId)}"
                : activity.Description
        );

        // Add thread link
        descriptionBuilder.AppendLine();
        descriptionBuilder.Append(
        $"""
        **⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯**
                                   **[Fil associé]({ChannelUtils.GetChannelLink(activity.GuildId, activity.ThreadId)})**
        **⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯**
        """);

        string bannerUrl = GetActivityBanner(activity.Name);

        var color = activity.IsClosed ?
                    Colors.CocotteRed : activityFull ?
                                        Colors.CocotteOrange : Colors.CocotteBlue;

        var builder = new EmbedBuilder()
            .WithColor(color)
            .WithTitle(title)
            .WithDescription(descriptionBuilder.ToString())
            .WithImageUrl(bannerUrl)
            .WithFields(fields);

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
        ActivityName.Minigame => "EV",
        ActivityName.MirroriaRace => "MR",
        _ => "NA"
    };

    public string FormatActivityPlayer(ActivityPlayer player, int namePadding, bool showOrganizerRole = false, bool hideRoles = false) =>
        player.HasCompleted
            ? $"~~{FormatActivityPlayerSub(player, namePadding, showOrganizerRole, hideRoles)}~~"
            : FormatActivityPlayerSub(player, namePadding, showOrganizerRole, hideRoles);

    private string FormatActivityPlayerSub(ActivityPlayer player, int namePadding, bool showOrganizerRole = false, bool hideRoles = false) => player switch
    {
        ActivityRolePlayer rolePlayer when !hideRoles => $"` {player.Name.PadRight(namePadding)} ` **|** {RolesToEmotes(rolePlayer.Roles)}",
        _ when showOrganizerRole && player.IsOrganizer => $"` {player.Name.PadRight(namePadding)} ` **|**  {_options.OrganizerEmote} ",
        _ => $"` {player.Name.PadRight(namePadding)} `"
    };

    private string RolesToEmotes(PlayerRoles rolePlayerRoles)
    {
        var emotesBuilder = new StringBuilder();

        if (rolePlayerRoles.HasFlag(PlayerRoles.Helper))
        {
            emotesBuilder.Append($" {_options.HelperEmote} **|**");
        }

        if (rolePlayerRoles.HasFlag(PlayerRoles.Support))
        {
            emotesBuilder.Append($" {_options.SupportEmote} ");
        }

        if (rolePlayerRoles.HasFlag(PlayerRoles.Tank))
        {
            emotesBuilder.Append($" {_options.TankEmote} ");
        }

        if (rolePlayerRoles.HasFlag(PlayerRoles.Dps))
        {
            emotesBuilder.Append($" {_options.DpsEmote} ");
        }

        return emotesBuilder.ToString();
    }
}