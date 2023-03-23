using Cocotte.Modules.Activities.Models;
using Cocotte.Utils;
using Discord;
using Discord.Interactions;

namespace Cocotte.Modules.Activities;

#if DEBUG
public partial class ActivityModule
{
    [MessageCommand("Add role player")]
    public async Task AddRolePlayer(IMessage message)
    {
        if (message is IUserMessage userMessage && userMessage.Author.IsBot)
        {
            if (await _activitiesRepository.FindActivity(Context.Guild.Id, message.Id) is { } activity)
            {
                // Generate random player
                var player = new ActivityRolePlayer
                {
                    Activity = activity,
                    Name = $"Player{Random.Shared.Next(1, 100)}",
                    UserId = (ulong) Random.Shared.NextInt64(),
                    Roles = (PlayerRoles) Random.Shared.Next((int) (PlayerRoles.Dps | PlayerRoles.Helper |
                                                                      PlayerRoles.Support | PlayerRoles.Tank) + 1)
                };

                // Add the player to the activity
                activity.ActivityPlayers.Add(player);
                await _activitiesRepository.SaveChanges();

                await UpdateActivityEmbed(activity, ActivityUpdateReason.PlayerJoin);
            }
        }

        await RespondAsync(
            embed: EmbedUtils.SuccessEmbed($"Successfully added a player").Build(),
            ephemeral: true
        );
    }
}
#endif