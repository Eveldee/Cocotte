﻿using Cocotte.Modules.Activities.Models;
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
            if (await _activitiesRepository.FindActivity(message.Id) is { } activity)
            {
                // Generate random player
                var player = new ActivityRolePlayer
                {
                    Activity = activity,
                    Name = $"Player{Random.Shared.Next(1, 100)}",
                    DiscordId = (ulong) Random.Shared.NextInt64(),
                    Roles = (ActivityRoles) Random.Shared.Next((int) (ActivityRoles.Dps | ActivityRoles.Helper |
                                                                      ActivityRoles.Support | ActivityRoles.Tank) + 1)
                };

                // Add the player to the activity
                activity.ActivityPlayers.Add(player);
                await _activitiesRepository.SaveChanges();

                await UpdateActivityEmbed(activity);
            }
        }

        await RespondAsync(
            embed: EmbedUtils.SuccessEmbed($"Successfully added a player").Build(),
            ephemeral: true
        );
    }
}
#endif