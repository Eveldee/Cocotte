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

    public static string FcFormat(uint fc) => fc switch
    {
        < 1_000 => $"{fc}",
        _ => $"{fc/1000}k"
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
            var rosterPlayers = players.ToList();
            var nonSubstitute = rosterPlayers.Where(p => !p.Substitute);
            var substitute = rosterPlayers.Where(p => p.Substitute);

            var separatorLength = Math.Max(nonSubstitute.Select(p => p.Name.Length).Max(), substitute.Select(p => p.Name.Length).Max());
            separatorLength = (int) ((separatorLength + 13) * 0.49); // Don't ask why, it just works

            return new EmbedFieldBuilder()
                .WithName($"Roster {rosterNumber}")
                .WithValue($"{string.Join("\n", nonSubstitute.Select(FormatRosterPlayer))}\n{new string('━', separatorLength)}\n{string.Join("\n", substitute.Select(FormatRosterPlayer))}")
                .WithIsInline(true);
        }

        return new EmbedBuilder()
            .WithColor(Colors.CocotteBlue)
            .WithTitle(":crossed_swords: Raid")
            .WithDescription($"**Date:** {TimestampTag.FromDateTime(raid.DateTime, TimestampTagStyles.LongDateTime)}")
            .WithFields(raid.Rosters.Select(r => RosterEmbed(r.Key, r)));
    }
}