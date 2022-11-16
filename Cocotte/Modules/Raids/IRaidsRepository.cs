namespace Cocotte.Modules.Raids;

public interface IRaidsRepository
{
    Raid this[ulong raidId] { get; }

    bool AddNewRaid(ulong raidId, DateTime dateTime);
}