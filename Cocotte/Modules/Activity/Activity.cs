namespace Cocotte.Modules.Activity;

public abstract record Activity(ulong Owner, string Description, ActivityType ActivityType, ActivityName ActivityName, uint MaxPlayers);
