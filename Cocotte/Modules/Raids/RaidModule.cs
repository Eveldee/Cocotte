using System.Diagnostics.CodeAnalysis;
using Cocotte.Utils;
using Discord;
using Discord.Interactions;

namespace Cocotte.Modules.Raids;

[Group("raid", "Raid related commands")]
public class RaidModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<RaidModule> _logger;
    private readonly IRaidsRepository _raidsRepository;

    public RaidModule(ILogger<RaidModule> logger, IRaidsRepository raidsRepository)
    {
        _logger = logger;
        _raidsRepository = raidsRepository;
    }

    [SlashCommand("start", "Start a raid formation")]
    public async Task Ping()
    {
        // Raids are identified using their original message id
        await RespondAsync("`Creating a new raid...`");

        var response = await GetOriginalResponseAsync();
        var raidId = response.Id;

        _logger.LogInformation("Created new raid with id {RaidId}", raidId);

        // New raid instance
        // TODO: Ask for date
        if (!_raidsRepository.AddNewRaid(raidId, DateTime.Now))
        {
            // A raid with this message id already exists, how??
            _logger.LogWarning("Tried to create a new raid with already existing id: {RaidId}", raidId);

            await FollowupAsync(ephemeral: true, embed: EmbedUtils.ErrorEmbed("Can't create a new raid with same raid id").Build());
            await DeleteOriginalResponseAsync();

            return;
        }

        // Build the raid message
        var embed = RaidEmbed(raidId);

        await ModifyOriginalResponseAsync(m =>
        {
            m.Content = "";
            m.Embed = embed.Build();
        });
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private EmbedBuilder RaidEmbed(ulong raidId)
    {
        EmbedFieldBuilder RosterEmbed(int rosterNumber, IEnumerable<RosterPlayer> players)
        {
            var nonSubstitute = players.Where(p => !p.Substitue);
            var substitute = players.Where(p => p.Substitue);

            var separatorLength = Math.Max(nonSubstitute.Select(p => p.Name.Length).Max(), substitute.Select(p => p.Name.Length).Max());
            separatorLength = (int) ((separatorLength + 13) * 0.49); // Don't ask why, it just works

            return new EmbedFieldBuilder()
                .WithName($"Roster {rosterNumber}")
                .WithValue($"{string.Join("\n", nonSubstitute)}\n{new string('━', separatorLength)}\n{string.Join("\n", substitute)}")
                .WithIsInline(true);
        }

        var raid = _raidsRepository[raidId];

        return new EmbedBuilder()
            .WithColor(Colors.CocotteBlue)
            .WithTitle(":crossed_swords: Raid")
            .WithDescription($"**Date:** {TimestampTag.FromDateTime(raid.DateTime, TimestampTagStyles.LongDateTime)}")
            .WithFields(raid.Rosters.Select(r => RosterEmbed(r.Key, r)));
    }
}