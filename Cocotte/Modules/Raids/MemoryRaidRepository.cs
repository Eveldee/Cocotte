namespace Cocotte.Modules.Raids;

public class MemoryRaidRepository : IRaidsRepository
{
    private readonly Dictionary<ulong, Raid> _raids;

    public Raid this[ulong raidId] => _raids[raidId];

    public MemoryRaidRepository()
    {
        _raids = new Dictionary<ulong, Raid>();
    }

    public bool AddNewRaid(ulong raidId, DateTime dateTime)
    {
        return _raids.TryAdd(raidId, new Raid(raidId, dateTime));
    }
}