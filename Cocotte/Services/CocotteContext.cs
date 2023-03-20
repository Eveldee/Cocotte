using Cocotte.Modules.Activity.Models;
using Microsoft.EntityFrameworkCore;

namespace Cocotte.Services;

public class CocotteContext : DbContext
{
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<StagedActivity> StagedActivities => Set<StagedActivity>();

    public DbSet<ActivityPlayer> ActivityPlayers => Set<ActivityPlayer>();
    public DbSet<ActivityRolePlayer> ActivityRolePlayers => Set<ActivityRolePlayer>();

    public CocotteContext(DbContextOptions<CocotteContext> options) : base(options)
    {

    }
}