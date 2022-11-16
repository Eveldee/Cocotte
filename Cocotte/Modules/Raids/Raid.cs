using Cocotte.Utils;

namespace Cocotte.Modules.Raids;

public class Raid
{
    public ulong Id { get; }
    public DateTime DateTime { get; }

    private readonly RosterManager _rosterManager = new();

    public IEnumerable<IGrouping<int, RosterPlayer>> Rosters => _rosterManager.Rosters;

    public Raid(ulong id, DateTime dateTime)
    {
        Id = id;
        DateTime = dateTime;

#if DEBUG
        this.AddTestPlayers();
#endif
    }

    public bool AddPlayer(string name, PlayerRole role, int fc, bool substitute = false)
    {
        return _rosterManager.AddPlayer(new RosterPlayer(name, role, fc, substitute));
    }

    public override bool Equals(object? other)
    {
        return other is Raid roster && roster.Id == Id;
    }

    public override int GetHashCode()
    {
        return (int) (Id % int.MaxValue);
    }
}