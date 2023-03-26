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
    - `/activite enlever <joueur>` - **Enlève un joueur** de cette activité

    - `/activite fermer` - **Ferme l'activité**, désactive les inscriptions
    - `/activite ouvrir` - **Ouvre l'activité**, réactive les inscriptions après une fermeture

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

        if (!await CheckCommandInThread(activity, checkCreator: true) || activity is null)
        {
            return;
        }

        await RemovePlayerFromActivity(activity, (SocketGuildUser) user, self: false);
    }

    [SlashCommand("fermer", "Fermer l'activité, désactivant les inscriptions")]
    public async Task ThreadCloseActivity()
    {
        // Get activity linked to this thread
        var activity = _activitiesRepository.FindActivityByThreadId(Context.Channel.Id);

        if (!await CheckCommandInThread(activity, checkCreator: true) || activity is null)
        {
            return;
        }

        // Do nothing if already closed
        if (activity.IsClosed)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.InfoEmbed("Cette activité est **déjà fermée**").Build()
            );

            return;
        }

        activity.IsClosed = true;
        await _activitiesRepository.SaveChanges();

        // Get activity channel to update
        if (Context.Guild.GetChannel(activity.ChannelId) is ITextChannel channel)
        {
            await _activityHelper.UpdateActivityEmbed(channel, activity, ActivityUpdateReason.Update);
        }

        await RespondAsync(
            ephemeral: true,
            embed: EmbedUtils.InfoEmbed("L'activité a bien été **fermée**").Build()
        );
    }

    [SlashCommand("ouvrir", "Ouvrir l'activité, réactivant les inscriptions")]
    public async Task ThreadOpenActivity()
    {
        // Get activity linked to this thread
        var activity = _activitiesRepository.FindActivityByThreadId(Context.Channel.Id);

        if (!await CheckCommandInThread(activity, checkCreator: true) || activity is null)
        {
            return;
        }

        // Do nothing if already opened
        if (!activity.IsClosed)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.InfoEmbed("Cette activité est **déjà ouverte**").Build()
            );

            return;
        }

        activity.IsClosed = false;
        await _activitiesRepository.SaveChanges();

        // Get activity channel to update
        if (Context.Guild.GetChannel(activity.ChannelId) is ITextChannel channel)
        {
            await _activityHelper.UpdateActivityEmbed(channel, activity, ActivityUpdateReason.Update);
        }

        await RespondAsync(
            ephemeral: true,
            embed: EmbedUtils.InfoEmbed("L'activité a bien été **ouverte**").Build()
        );
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

    private async Task<bool> CheckCommandInThread(Activity? activity, bool checkCreator = false)
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

        if (checkCreator && User.Id != activity.CreatorUserId)
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedUtils.ErrorEmbed("Seul le **créateur de l'activité** a le droit d’exécuter cette action").Build()
            );

            return false;
        }

        return true;
    }
}