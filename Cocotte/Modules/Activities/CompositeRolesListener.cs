using Cocotte.Options;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Cocotte.Modules.Activities;

public class CompositeRolesListener
{
    private readonly ILogger<CompositeRolesListener> _logger;
    private readonly IReadOnlyDictionary<ulong, GuildCompositeRoles[]> _compositeRoles;

    public CompositeRolesListener(ILogger<CompositeRolesListener> logger,
        IOptions<CompositeRolesOptions> compositeRolesOptions)
    {
        _logger = logger;
        _compositeRoles = compositeRolesOptions.Value.CompositeRoles;
    }

    public async Task UserUpdated(Cacheable<SocketGuildUser, ulong> cacheable, SocketGuildUser guildUser)
    {
        // Fetch composite roles for this guild
        if (!_compositeRoles.TryGetValue(guildUser.Guild.Id, out var guildCompositeRoles))
        {
            return;
        }

        _logger.LogTrace("Guild {Guild} has at least one composite role, checking for user {User}", guildUser.Guild.Name, guildUser.DisplayName);

        // Check roles for each composite roles
        var roles = guildUser.Roles;
        foreach (var compositeRole in guildCompositeRoles)
        {
            // If the user has the target role, check if we need to remove it
            if (roles.FirstOrDefault(r => r.Id == compositeRole.TargetRoleId) is { } presentTargetRole)
            {
                // Check that the user no associated role
                if (!roles.Any(r => compositeRole.CompositeRolesIds.Contains(r.Id)))
                {
                    await guildUser.RemoveRoleAsync(presentTargetRole);

                    _logger.LogInformation("CompositeRoles removed role {Role} from {User}", presentTargetRole.Name, guildUser.DisplayName);
                }
            }
            // It the user doesn't have the target role, check if we need to add it
            else
            {
                // Check that the user has at least one of the desired roles
                if (roles.Any(r => compositeRole.CompositeRolesIds.Contains(r.Id)))
                {
                    var missingTargetRole = guildUser.Guild.GetRole(compositeRole.TargetRoleId);
                    await guildUser.AddRoleAsync(missingTargetRole);

                    _logger.LogInformation("CompositeRoles added role {Role} from {User}", missingTargetRole.Name, guildUser.DisplayName);
                }
            }
        }
    }
}