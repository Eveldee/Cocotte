using System.Numerics;
using Cocotte.Options;
using Cocotte.Utils;
using Discord;

namespace Cocotte.Modules.Raids;

public class RaidFormatter
{
    private readonly RolesOptions _rolesOptions;

    public RaidFormatter(RolesOptions rolesOptions)
    {
        _rolesOptions = rolesOptions;
    }

    public IEmote RoleToEmote(PlayerRole role) => role switch
    {
        PlayerRole.Dps => _rolesOptions.DpsEmote.ToEmote(),
        PlayerRole.Tank => _rolesOptions.TankEmote.ToEmote(),
        PlayerRole.Healer => _rolesOptions.HealerEmote.ToEmote(),
        _ => ":question:".ToEmote()
    };

    public static string FcFormat<T>(T fc) where T : IBinaryInteger<T> => fc switch
    {
        < 1_000 => $"{fc}",
        _ => $"{fc/T.CreateChecked(1000)}k"
    };

    public string FormatRosterPlayer(RosterPlayer player) => player.Substitute switch
    {
        false => $"{RoleToEmote(player.Role)} {player.Name} ({FcFormat(player.Fc)} FC)",
        true => $"*{RoleToEmote(player.Role)} {player.Name} ({FcFormat(player.Fc)} FC)*"
    };

    public EmbedBuilder RaidEmbed(Raid raid)
    {
        EmbedFieldBuilder RosterEmbed(int rosterNumber, IEnumerable<RosterPlayer> players)
        {
            var rosterPlayers = players.OrderByDescending(p => p.Role).ThenByDescending(p => p.Fc).ToList();
            var nonSubstitute = rosterPlayers.Where(p => !p.Substitute);
            var substitute = rosterPlayers.Where(p => p.Substitute);

            var separatorLength = players.Select(p => p.Name.Length).Max();
            separatorLength = (int) ((separatorLength + 5) * 1.31); // Don't ask why, it just works

            return new EmbedFieldBuilder()
                .WithName($"Roster {rosterNumber} ({FcFormat(nonSubstitute.Sum(p => p.Fc))} FC)")
                .WithValue($"{string.Join("\n", nonSubstitute.Select(FormatRosterPlayer))}\n{new string('━', separatorLength)}\n{string.Join("\n", substitute.Select(FormatRosterPlayer))}")
                .WithIsInline(true);
        }

        return new EmbedBuilder()
            .WithColor(Colors.CocotteBlue)
            .WithTitle(":crossed_swords: Raid")
            .WithDescription($"**Date:** {TimestampTag.FromDateTime(raid.DateTime, TimestampTagStyles.LongDateTime)}")
            .WithFields(raid.Rosters.OrderBy(r => r.Key).Select(r => RosterEmbed(r.Key, r)));
    }
}