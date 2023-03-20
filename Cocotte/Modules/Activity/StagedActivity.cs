namespace Cocotte.Modules.Activity;

public record StagedActivity(ulong Owner, string Description, ActivityType ActivityType, ActivityName ActivityName, uint MaxPlayers, uint Stage)
    : Activity(Owner, Description, ActivityType, ActivityName, MaxPlayers);