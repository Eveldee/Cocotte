using Cocotte.Modules.Activities.Models;
using Discord.WebSocket;
using Quartz;

namespace Cocotte.Modules.Activities;

public class ActivityCloseJob : IJob
{
    public long GuildId { private get; set; }
    public long ChannelId { private get; set; }
    public long MessageId { private get; set; }

    private readonly ILogger<ActivityCloseJob> _logger;
    private readonly ActivitiesRepository _activitiesRepository;
    private readonly ActivityHelper _activityHelper;
    private readonly DiscordSocketClient _discordClient;

    public ActivityCloseJob(ILogger<ActivityCloseJob> logger, ActivitiesRepository activitiesRepository, ActivityHelper activityHelper, DiscordSocketClient discordClient)
    {
        _logger = logger;
        _activitiesRepository = activitiesRepository;
        _activityHelper = activityHelper;
        _discordClient = discordClient;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        // Get associated activity
        if (await _activitiesRepository.FindActivity((ulong)GuildId, (ulong)ChannelId, (ulong)MessageId) is not { } activity)
        {
            _logger.LogTrace("Activity {MessageId} does not exist anymore", MessageId);

            return;
        }

        // Close activity
        activity.IsClosed = true;
        await _activitiesRepository.SaveChanges();

        _logger.LogInformation("Closed activity {Activity}", activity);

        // Get channel
        if (_discordClient.GetChannel(activity.ChannelId) is not SocketTextChannel channel)
        {
            return;
        }

        // Update embed
        await _activityHelper.UpdateActivityEmbed(channel, activity, ActivityUpdateReason.Update);
    }
}