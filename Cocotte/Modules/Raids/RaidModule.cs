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
    private readonly IRaidsRepository _raids;
    private readonly IPlayerInfosRepository _playerInfos;
    private readonly RaidFormatter _raidFormatter;
    private readonly RaidRegisterManager _registerManager;

    public RaidModule(ILogger<RaidModule> logger, IRaidsRepository raids, IPlayerInfosRepository playerInfos,
        RaidFormatter raidFormatter, RaidRegisterManager registerManager)
    {
        _logger = logger;
        _raids = raids;
        _playerInfos = playerInfos;
        _raidFormatter = raidFormatter;
        _registerManager = registerManager;
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
        var date = DateTime.Today;
        date = date.AddDays(DateTimeUtils.CalculateDayOfWeekOffset(date.DayOfWeek, day))
                   .Add(timeOnly.ToTimeSpan());

        // New raid instance
        if (!_raids.AddNewRaid(raidId, date))
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
        var raid = _raids[raidId];
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
    public async Task RaidJoin(ulong raidId)
    {
        if (!_raids.TryGetRaid(raidId, out var raid))
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("This raid does not exist").Build()
            );

            return;
        }

        var user = (IGuildUser) Context.User;

        // Check if player is already registered early
        if (raid.ContainsPlayer(user.Id))
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.InfoEmbed("You're already registered for this raid").Build());

            return;
        }

        // Ask player info
        _registerManager.RegisteringPlayers[(raidId, user.Id)] = new RosterPlayer(user.Id, user.DisplayName);

        _logger.LogDebug("User {User} is registering for raid {Raid}", user.Username, raid);

        // Add name
        await RespondAsync($"Please select a role for raid", components: PlayerRoleComponent(raid, user).Build(), ephemeral: true);
    }

    [ComponentInteraction("raid player_select_role:*:*", ignoreGroupNames: true)]
    public async Task PlayerSelectRole(ulong raidId, ulong playerId, string selectedRoleInput)
    {
        var selectedRole = Enum.Parse<PlayerRole>(selectedRoleInput);

        if (_registerManager.RegisteringPlayers.TryGetValue((raidId, playerId), out var rosterPlayer))
        {
            _registerManager.RegisteringPlayers[(raidId, playerId)] = rosterPlayer with {Role = selectedRole};

            await RespondAsync();
        }
        // The user is not currently registering, wonder how he got here then
        else
        {
            await RespondAsync(ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("You are not registering for this raid :thinking:").Build());
        }
    }

    [ComponentInteraction("raid player_join:*:*", ignoreGroupNames: true)]
    public async Task PlayerJoinNonSubstitute(ulong raidId, ulong playerId)
    {
        await PlayerJoin(raidId, playerId, false);
    }

    [ComponentInteraction("raid player_join_substitute:*:*", ignoreGroupNames: true)]
    public async Task PlayerJoinSubstitute(ulong raidId, ulong playerId)
    {
        await PlayerJoin(raidId, playerId, true);
    }

    private async Task PlayerJoin(ulong raidId, ulong playerId, bool substitute)
    {
        // Check if player is registering
        if (!_registerManager.RegisteringPlayers.TryGetValue((raidId, playerId), out var rosterPlayer))
        {
            await RespondAsync(ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("You are not registering for this raid :thinking:").Build());

            return;
        }

        // Check if we need to ask FC
        if (!_playerInfos.TryGetPlayerInfo(playerId, out var playerInfo))
        {
            await Context.Interaction.RespondWithModalAsync<FcModal>($"raid modal_fc:{raidId}");

            return;
        }

        // Player already has FC registered but it's outdated
        if (playerInfo.IsFcUpdateRequired)
        {
            await Context.Interaction.RespondWithModalAsync<FcModal>($"raid modal_fc:{raidId}",
                modifyModal: m =>
                    m.UpdateTextInput("fc", component => component.Value = playerInfo.Fc.FormatSpaced())
            );

            return;
        }

        // Register user for raid
        await RegisterPlayer(raidId, rosterPlayer with { Fc = rosterPlayer.Fc, Substitute = substitute});
    }

    [ModalInteraction("raid modal_fc:*", ignoreGroupNames: true)]
    public async Task ModalFcSubmit(ulong raidId, FcModal modal)
    {
        var playerId = Context.User.Id;
        _logger.LogTrace("Received modal FC modal from {User} with value: {Fc}", Context.User.Username, modal.Fc);

        // Check if player is registering
        if (!_registerManager.RegisteringPlayers.TryGetValue((raidId, playerId), out var rosterPlayer))
        {
            await RespondAsync(ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("You are not registering for this raid :thinking:").Build());

            return;
        }

        var fcInput = modal.Fc.Replace(" ", "");

        if (!uint.TryParse(fcInput, out var fc))
        {
            await RespondAsync(ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("Invalid fc, try registering again").Build());

            _registerManager.RegisteringPlayers.Remove((raidId, playerId));

            return;
        }

        _playerInfos.UpdatePlayerInfo(new PlayerInfo(playerId, fc));

        await RegisterPlayer(raidId, rosterPlayer with { Fc = fc });
    }

    private async Task RegisterPlayer(ulong raidId, RosterPlayer rosterPlayer)
    {
        // Check if raid exists
        if (!_raids.TryGetRaid(raidId, out var raid))
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.InfoEmbed("This raid does not exist").Build());

            return;
        }

        _registerManager.RegisteringPlayers[(raidId, Context.User.Id)] = rosterPlayer;

        // Player is already registered, update info
        if (!raid.AddPlayer(rosterPlayer))
        {
            raid.UpdatePlayer(rosterPlayer);

            await UpdateRaidRosterEmbed(raid);
            // await RespondAsync(
            //     ephemeral: true,
            //     embed: EmbedUtils.SuccessEmbed($"Successfully update you're role to: {rosterPlayer.Role}").Build());
            await RespondAsync();
        }
        // It's a new player
        else
        {
            _logger.LogInformation("User {User} joined raid {Raid}", Context.User.Username, raid);

            await UpdateRaidRosterEmbed(raid);
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.SuccessEmbed($"Successfully joined the raid as {raid.GetPlayer(rosterPlayer.Id).Role}").Build()
            );
        }
    }

    [ComponentInteraction("raid raid_leave:*", ignoreGroupNames: true)]
    public async Task Leave(ulong raidId)
    {
        if (!_raids.TryGetRaid(raidId, out var raid))
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

    private static ComponentBuilder RaidComponents(ulong raidId)
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

    private ComponentBuilder PlayerRoleComponent(Raid raid, IGuildUser user)
    {
        var select = new SelectMenuBuilder()
            .WithPlaceholder(PlayerRole.Dps.ToString())
            .WithCustomId($"raid player_select_role:{raid.Id}:{user.Id}")
            .WithMinValues(1)
            .WithMaxValues(1);

        foreach (var role in Enum.GetValues<PlayerRole>())
        {
            // TODO add emote
            select.AddOption(role.ToString(), role.ToString());
        }

        return new ComponentBuilder()
            .AddRow(new ActionRowBuilder()
                .WithSelectMenu(select)
            )
            .AddRow(new ActionRowBuilder()
                .WithButton("Join", $"raid player_join:{raid.Id}:{user.Id}")
                .WithButton("Join substitute", $"raid player_join_substitute:{raid.Id}:{user.Id}")
            );
    }
}

public class FcModal : IModal
{
    public string Title => "Please enter your FC";

    [NotNull]
    [InputLabel("FC")]
    [ModalTextInput("fc", placeholder: "30 000", minLength: 1, maxLength: 7)]
    public string? Fc { get; set; }
}