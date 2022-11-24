using System.Diagnostics.CodeAnalysis;
using Cocotte.Utils;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

// ReSharper disable UnusedMember.Global

namespace Cocotte.Modules.Raids;

[Group("raid", "Raid related commands")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class RaidModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<RaidModule> _logger;
    private readonly IRaidsRepository _raidsRepository;
    private readonly RaidFormatter _raidFormatter;

    public RaidModule(ILogger<RaidModule> logger, IRaidsRepository raidsRepository, RaidFormatter raidFormatter)
    {
        _logger = logger;
        _raidsRepository = raidsRepository;
        _raidFormatter = raidFormatter;
    }

    [EnabledInDm(false)]
    [SlashCommand("start", "Start a raid formation")]
    public async Task Start(DayOfWeek day, string time)
    {
        // Check if time is valid
        if (!TimeOnly.TryParse(time, out var timeOnly))
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("Invalid time, try using format 'hh:mm'").Build()
            );

            return;
        }

        // Raids are identified using their original message id
        await RespondAsync("`Creating a new raid...`");

        var response = await GetOriginalResponseAsync();
        var raidId = response.Id;

        _logger.LogInformation("Created new raid with id {RaidId}", raidId);

        // Calculate date
        var date = DateTime.Now.Date;
        date = date.AddDays(DateTimeUtils.CalculateDayOfWeekOffset(date.DayOfWeek, day))
                   .Add(timeOnly.ToTimeSpan());

        // New raid instance
        if (!_raidsRepository.AddNewRaid(raidId, date))
        {
            // A raid with this message id already exists, how??
            _logger.LogWarning("Tried to create a new raid with already existing id: {RaidId}", raidId);

            await FollowupAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("Can't create a new raid with same raid id").Build()
            );
            await DeleteOriginalResponseAsync();

            return;
        }

        // Build the raid message
        var raid = _raidsRepository[raidId];
        var components = RaidComponents(raidId);
        var embed = _raidFormatter.RaidEmbed(raid);

        await ModifyOriginalResponseAsync(m =>
        {
            m.Content = "";
            m.Components = components.Build();
            m.Embed = embed.Build();
        });
    }

    [ComponentInteraction("raid raid_join:*", true)]
    public async Task Join(ulong raidId)
    {
        if (!_raidsRepository.TryGetRaid(raidId, out var raid))
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("This raid does not exist").Build()
            );

            return;
        }

        // Todo: Ask role, FC, substitute
        var user = (IGuildUser) Context.User;
        if (!raid.AddPlayer(user.Id, user.DisplayName, PlayerRole.Dps, 10000))
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.InfoEmbed("You're already registered for this raid").Build());

            return;
        }

        _logger.LogInformation("User {User} joined raid {Raid}", Context.User.Username, raid);

        await UpdateRaidRosterEmbed(raid);
        await RespondAsync(
            ephemeral: true,
            embed: EmbedUtils.SuccessEmbed($"Successfully joined the raid as {raid.GetPlayer(user.Id).Role}").Build()
        );
    }

    [ComponentInteraction("raid raid_leave:*", ignoreGroupNames: true)]
    public async Task Leave(ulong raidId)
    {
        if (!_raidsRepository.TryGetRaid(raidId, out var raid))
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("This raid does not exist").Build()
            );

            return;
        }

        var user = (IGuildUser) Context.User;
        if (!raid.RemovePlayer(user.Id))
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.WarningEmbed("You're not registered for this raid").Build()
            );

            return;
        }

        await UpdateRaidRosterEmbed(raid);
        await RespondAsync(
            ephemeral: true,
            embed: EmbedUtils.SuccessEmbed("Successfully left the raid").Build()
        );
    }

    public async Task UpdateRaidRosterEmbed(Raid raid)
    {
        var message = await Context.Channel.GetMessageAsync(raid.Id);

        if (message is SocketUserMessage userMessage)
        {
            await userMessage.ModifyAsync(
                m => m.Embed = _raidFormatter.RaidEmbed(raid).Build()
            );
        }
    }

    private ComponentBuilder RaidComponents(ulong raidId)
    {
        return new ComponentBuilder()
            .AddRow(new ActionRowBuilder()
                .WithButton(new ButtonBuilder()
                    .WithLabel("Join")
                    .WithCustomId($"raid raid_join:{raidId}")
                    .WithStyle(ButtonStyle.Primary)
                )
                .WithButton(new ButtonBuilder()
                    .WithLabel("Leave")
                    .WithCustomId($"raid raid_leave:{raidId}")
                    .WithStyle(ButtonStyle.Danger)
                )
            );
    }
}