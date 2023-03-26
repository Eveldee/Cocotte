using AppAny.Quartz.EntityFrameworkCore.Migrations;
using AppAny.Quartz.EntityFrameworkCore.Migrations.SQLite;
using Cocotte.Modules.Activities.Models;
using Microsoft.EntityFrameworkCore;

namespace Cocotte.Services;

public class CocotteDbContext : DbContext
{
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<StagedActivity> StagedActivities => Set<StagedActivity>();
    public DbSet<InterstellarActivity> InterstellarActivities => Set<InterstellarActivity>();

    public DbSet<ActivityPlayer> ActivityPlayers => Set<ActivityPlayer>();
    public DbSet<ActivityRolePlayer> ActivityRolePlayers => Set<ActivityRolePlayer>();

    public CocotteDbContext(DbContextOptions<CocotteDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Adds Quartz.NET SQLite schema to EntityFrameworkCore
        modelBuilder.AddQuartz(builder => builder.UseSqlite());
    }
}