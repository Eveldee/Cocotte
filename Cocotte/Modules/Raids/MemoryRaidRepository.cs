using System.Diagnostics.CodeAnalysis;

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

    public bool TryGetRaid(ulong raidId, [MaybeNullWhen(false)] out Raid raid)
    {
        return _raids.TryGetValue(raidId, out raid);
    }
}