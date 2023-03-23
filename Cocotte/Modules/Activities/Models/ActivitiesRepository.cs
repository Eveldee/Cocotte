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

    public async Task<Activity?> FindActivity(ulong guildId, ulong activityId)
    {
        return await _cocotteDbContext.Activities.FindAsync(guildId, activityId);
    }

    public async Task<ActivityPlayer?> FindActivityPlayer(ulong guildId, ulong activityId, ulong playerId)
    {
        return await _cocotteDbContext.ActivityPlayers.FindAsync(guildId, activityId, playerId);
    }

    public async Task<int> ActivityPlayerCount(Activity activity) =>
        await _cocotteDbContext.ActivityPlayers.Where(player => player.ActivityId == activity.ActivityId).CountAsync();

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
}