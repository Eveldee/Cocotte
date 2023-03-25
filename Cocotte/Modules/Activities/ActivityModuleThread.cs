using System.Text;
using Cocotte.Modules.Activities.Models;
using Cocotte.Utils;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Cocotte.Modules.Activities;

public partial class ActivityModule
{
    private string GetStartMessage(ActivityName activityName, string creatorName) =>
    $"""
    **― Bienvenue ―**
    Bienvenue sur le thread lié à l'activité **{_activityFormatter.FormatActivityName(activityName)}** de **{creatorName}**

    Ici, vous pouvez **discuter** de l'activité, mais aussi **gérer** cette activité à l'aide de diverses **commandes**.

    **― Commandes ―**
    - `/activite ajouter <joueur>` - **Ajoute un joueur** à cette activité
    - `/activite supprimer <joueur>` - **Supprime un joueur** de cette activité
    - `/activite ping` - **Ping les joueurs** inscrits à cette activité
    - `/activite description` - **Modifie la description** de l'activité
    - `/activite etage` - Pour l'abîme du néant et l'origine de la guerre, **modifie l'étage** de l'activité
    """;

    private async Task<ulong> CreateThread(ActivityName activityName, string creatorName)
    {
        var channel = (SocketTextChannel) Context.Channel;
        var message = await GetOriginalResponseAsync();

        // Create thread
        var thread = await channel.CreateThreadAsync(
            $"{_activityFormatter.FormatActivityName(activityName)} - {creatorName}", ThreadType.PublicThread,
            ThreadArchiveDuration.OneHour,
            message, true
        );

        // Send management message
        await thread.SendMessageAsync(GetStartMessage(activityName, creatorName));

        // Add activity creator
        await thread.AddUserAsync((IGuildUser) Context.User);

        return thread.Id;
    }

    [SlashCommand("ajouter", "Ajouter un joueur à cette activité")]
    public async Task ThreadAddPlayer(IUser user)
    {
        // Get activity linked to this thread
        var activity = _activitiesRepository.FindActivityByThreadId(Context.Channel.Id);

        if (!await CheckCommandInThread(activity) || activity is null)
        {
            return;
        }

        await AddUserToActivity(activity, (SocketGuildUser) user, self: false);
    }

    [SlashCommand("enlever", "Enlever un joueur de cette activité")]
    public async Task ThreadRemovePlayer(IUser user)
    {
        // Get activity linked to this thread
        var activity = _activitiesRepository.FindActivityByThreadId(Context.Channel.Id);

        if (!await CheckCommandInThread(activity) || activity is null)
        {
            return;
        }

        await RemovePlayerFromActivity(activity, (SocketGuildUser) user, self: false);
    }

    [SlashCommand("ping", "Ping les joueurs inscrits à cette activité")]
    public async Task ThreadPingPlayers(string message = "**Appel de groupe**")
    {
        // Get activity linked to this thread
        var activity = _activitiesRepository.FindActivityByThreadId(Context.Channel.Id);

        if (!await CheckCommandInThread(activity) || activity is null)
        {
            return;
        }

        // Get user ids
        var userIds = await _activitiesRepository.GetActivityPlayerIds(activity);


        // Generate message
        var pingMessageBuilder = new StringBuilder(message);
        pingMessageBuilder.AppendLine("\n");
        pingMessageBuilder.Append(string.Join(", ", userIds.Select(id => MentionUtils.MentionUser(id))));

        await RespondAsync(pingMessageBuilder.ToString());
    }

    private async Task<bool> CheckCommandInThread(Activity? activity)
    {
        // Check if activity is not null (means we are in a valid thread)
        if (activity is null)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("Vous devez être dans un **thread lié à une activité** pour utiliser cette commande").Build()
            );

            return false;
        }

        return true;
    }
}