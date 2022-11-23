namespace Cocotte.Modules.Raids;

public class RosterManager
{
    private readonly IDictionary<ulong, RosterPlayer> _players = new Dictionary<ulong, RosterPlayer>();

    public IEnumerable<IGrouping<int, RosterPlayer>> Rosters => _players.Select(p => p.Value).GroupBy(p => p.RosterNumber);

    public bool AddPlayer(RosterPlayer rosterPlayer)
    {
        // TODO add logic to split player in multiple rosters
        rosterPlayer.RosterNumber = 1;

        return _players.TryAdd(rosterPlayer.Id, rosterPlayer);
    }

    public bool RemovePlayer(ulong id)
    {
        return _players.Remove(id);
    }

    public RosterPlayer GetPlayer(ulong id)
    {
        return _players[id];
    }
}