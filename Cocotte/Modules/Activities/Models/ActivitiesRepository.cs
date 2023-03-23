using Cocotte.Services;
using Microsoft.EntityFrameworkCore;

namespace Cocotte.Modules.Activities.Models;

public class ActivitiesRepository
{
    private readonly CocotteDbContext _cocotteDbContext;

    public ActivitiesRepository(CocotteDbContext cocotteDbContext)
    {
        _cocotteDbContext = cocotteDbContext;
    }

    public async Task<Activity?> FindActivity(ulong guildId, ulong channelId, ulong messageId)
    {
        return await _cocotteDbContext.Activities.FindAsync(guildId, channelId, messageId);
    }

    public async Task<ActivityPlayer?> FindActivityPlayer(ulong guildId, ulong channelId, ulong messageId, ulong userId)
    {
        return await _cocotteDbContext.ActivityPlayers.FindAsync(guildId, channelId, messageId, userId);
    }

    public int ActivityPlayerCount(Activity activity) => activity.ActivityPlayers.Count;

    public async Task<IReadOnlyCollection<ActivityPlayer>> LoadActivityPlayers(Activity activity)
    {
        await _cocotteDbContext
            .Entry(activity)
            .Collection(a => a.ActivityPlayers)
            .LoadAsync();

        return activity.ActivityPlayers;
    }

    public async Task AddActivity(Activity activity)
    {
        await _cocotteDbContext.AddAsync(activity);
    }

    public async Task SaveChanges()
    {
        await _cocotteDbContext.SaveChangesAsync();
    }

    public void DeleteActivity(Activity activity)
    {
        _cocotteDbContext.Activities.Remove(activity);
    }

    public Activity? FindActivityByThreadId(ulong threadId)
    {
        return _cocotteDbContext.Activities.FirstOrDefault(activity => activity.ThreadId == threadId);
    }

    public async Task<bool> ActivityContainsUser(Activity activity, ulong userId)
    {
        return await _cocotteDbContext.Entry(activity)
            .Collection(a => a.ActivityPlayers)
            .Query()
            .AnyAsync(p => p.UserId == userId);
    }
}