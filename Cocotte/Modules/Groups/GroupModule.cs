using Cocotte.Options;
using Cocotte.Utils;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Options;

namespace Cocotte.Modules.Groups;

/// <summary>
/// Module to ask and propose groups for different activities: Abyss, OOW, FC, ...
/// </summary>
[Group("group", "Group related commands")]
public class GroupModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<GroupModule> _logger;
    private readonly GroupsOptions _options;

    public GroupModule(ILogger<GroupModule> logger, IOptions<GroupsOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    [RequireOwner]
    [SlashCommand("test", "Test group module")]
    public async Task Test()
    {
        await RespondAsync("Module is active!!");
    }

    [RequireOwner]
    [SlashCommand("setup-info", "Display group setup info")]
    public async Task SetupInfo()
    {
        await RespondAsync($"""
        - Helper: {MentionUtils.MentionRole(_options.HelperRoleId)} {_options.HelperEmote.ToEmote()}
        - Dps: {MentionUtils.MentionRole(_options.DpsRoleId)} {_options.DpsEmote.ToEmote()}
        - Tank: {MentionUtils.MentionRole(_options.TankRoleId)} {_options.TankEmote.ToEmote()}
        - Healer: {MentionUtils.MentionRole(_options.HealerRoleId)} {_options.HealerEmote.ToEmote()}
        """);
    }
}
