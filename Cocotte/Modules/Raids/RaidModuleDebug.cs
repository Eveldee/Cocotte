using Cocotte.Utils;
using Discord;
using Discord.Interactions;

namespace Cocotte.Modules.Raids;

#if DEBUG
public partial class RaidModule
{

    [MessageCommand("Add dps")]
    public async Task AddDps(IMessage message)
    {
        await AddTestPlayer(message, PlayerRole.Dps);
    }

    [MessageCommand("Add tank")]
    public async Task AddTank(IMessage message)
    {
        await AddTestPlayer(message, PlayerRole.Tank);
    }

    [MessageCommand("Add healer")]
    public async Task AddHealer(IMessage message)
    {
        await AddTestPlayer(message, PlayerRole.Healer);
    }

    [MessageCommand("Fill roster")]
    public async Task FillRoster(IMessage message)
    {
        if (message is IUserMessage userMessage && userMessage.Author.IsBot)
        {
            if (_raids.TryGetRaid(userMessage.Id, out var raid))
            {
                // Add 3 healers
                for (int i = 0; i < 3; i++)
                {
                    raid.AddPlayer(new RosterPlayer(
                        (ulong) Random.Shared.NextInt64(),
                        $"Healer{Random.Shared.Next(1, 100)}",
                        PlayerRole.Healer,
                        (uint) (1000 * Random.Shared.Next(30, 60)))
                    );
                }

                // Add 3 tanks
                for (int i = 0; i < 3; i++)
                {
                    raid.AddPlayer(new RosterPlayer(
                        (ulong) Random.Shared.NextInt64(),
                        $"Tank{Random.Shared.Next(1, 100)}",
                        PlayerRole.Tank,
                        (uint) (1000 * Random.Shared.Next(30, 60)))
                    );
                }

                // Add 8 dps
                for (int i = 0; i < 8; i++)
                {
                    raid.AddPlayer(new RosterPlayer(
                        (ulong) Random.Shared.NextInt64(),
                        $"Dps{Random.Shared.Next(1, 100)}",
                        PlayerRole.Dps,
                        (uint) (1000 * Random.Shared.Next(30, 60)))
                    );
                }

                // Fill rest with substitutes
                for (int i = 0; i < 6; i++)
                {
                    raid.AddPlayer(new RosterPlayer(
                        (ulong) Random.Shared.NextInt64(),
                        $"Dps{Random.Shared.Next(1, 100)}",
                        PlayerRole.Dps,
                        (uint) (1000 * Random.Shared.Next(30, 60)),
                        true)
                    );
                }

                await UpdateRaidRosterEmbed(raid);
            }
        }

        await RespondAsync(
            embed: EmbedUtils.SuccessEmbed("Successfully filled the roster").Build(),
            ephemeral: true
        );
    }

    private async Task AddTestPlayer(IMessage message, PlayerRole playerRole)
    {
        if (message is IUserMessage userMessage && userMessage.Author.IsBot)
        {
            if (_raids.TryGetRaid(userMessage.Id, out var raid))
            {
                raid.AddPlayer(new RosterPlayer(
                    (ulong) Random.Shared.NextInt64(),
                    $"Player{Random.Shared.Next(1, 100)}",
                    playerRole,
                    (uint) (1000 * Random.Shared.Next(30, 60)),
                    Random.Shared.Next(0, 2) == 0)
                );

                await UpdateRaidRosterEmbed(raid);
            }
        }

        await RespondAsync(
            embed: EmbedUtils.SuccessEmbed($"Successfully added a {playerRole} player").Build(),
            ephemeral: true
        );
    }
}
#endif