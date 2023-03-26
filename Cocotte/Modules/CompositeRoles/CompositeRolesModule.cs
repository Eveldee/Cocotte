using Cocotte.Options;
using Cocotte.Utils;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Cocotte.Modules.CompositeRoles;

[Group("autoroles", "Commandes liées aux rôles composés")]
public class CompositeRolesModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<CompositeRolesModule> _logger;
    private readonly CompositeRolesOptions _compositeRolesOptions;

    public CompositeRolesModule(ILogger<CompositeRolesModule> logger, IOptions<CompositeRolesOptions> options)
    {
        _logger = logger;

        _compositeRolesOptions = options.Value;
    }

    [RequireOwner]
    [SlashCommand("fix", "Réattribuer les roles composés")]
    public async Task FixRoles()
    {
        var count = 0;

        await RespondAsync("***`Checking autoroles for guild users...`***", ephemeral: true);

        // Check if there's composite roles for this guild
        if (!_compositeRolesOptions.CompositeRoles.TryGetValue(Context.Guild.Id, out var compositeRoles))
        {
            await ModifyOriginalResponseAsync(properties =>
            {
                properties.Content = "";
                properties.Embed = EmbedUtils.InfoEmbed("Il n'y a pas de rôle composé pour ce serveur").Build();
            });

            return;
        }

        // Check for all guild members
        await Context.Guild.DownloadUsersAsync();
        foreach (var guildUser in Context.Guild.Users)
        {
            var roles = ((SocketGuildUser)guildUser).Roles;

            // Check for each target role if they have at least one of the composite roles
            foreach (var compositeRole in compositeRoles)
            {
                // If the user has the target role, check if we need to remove it
                if (roles.FirstOrDefault(r => r.Id == compositeRole.TargetRoleId) is { } presentTargetRole)
                {
                    // Check that the user no associated role
                    if (!roles.Any(r => compositeRole.CompositeRolesIds.Contains(r.Id)))
                    {
                        await guildUser.RemoveRoleAsync(presentTargetRole);

                        _logger.LogInformation("CompositeRoles removed role {Role} from {User}", presentTargetRole.Name, guildUser.DisplayName);

                        count++;
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

                        count++;
                    }
                }
            }
        }

        await ModifyOriginalResponseAsync(properties =>
        {
            properties.Content = "";
            properties.Embed = EmbedUtils.InfoEmbed($"Les rôles de **{count}** utilisateurs ont été mis à jour").Build();
        });
    }
}