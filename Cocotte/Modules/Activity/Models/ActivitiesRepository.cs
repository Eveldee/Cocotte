using Cocotte.Services;
using Microsoft.EntityFrameworkCore;

namespace Cocotte.Modules.Activity.Models;

public class ActivitiesRepository
{
    private readonly CocotteContext _cocotteContext;

    public ActivitiesRepository(CocotteContext cocotteContext)
    {
        _cocotteContext = cocotteContext;
    }

    public async Task<Activity?> FindActivity(ulong activityId)
    {
        return await _cocotteContext.Activities.FindAsync(activityId);
    }

    public async Task<StagedActivity?> FindStagedActivity(ulong activityId)
    {
        return await _cocotteContext.StagedActivities.FindAsync(activityId);
    }

    public async Task<ActivityPlayer?> FindActivityPlayer(ulong activityId, ulong playerId)
    {
        return await _cocotteContext.ActivityPlayers.FindAsync(activityId, playerId);
    }

    public async Task<ActivityRolePlayer?> FindActivityRolePlayer(ulong activityId, ulong playerId)
    {
        return await _cocotteContext.ActivityRolePlayers.FindAsync(activityId, playerId);
    }

    public async Task AddActivity(Activity activity)
    {
        await _cocotteContext.AddAsync(activity);
        await _cocotteContext.SaveChangesAsync();
    }

    public async Task SaveChanges()
    {
        await _cocotteContext.SaveChangesAsync();
    }
}