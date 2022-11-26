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

    public bool UpdatePlayer(RosterPlayer rosterPlayer)
    {
        if (!_players.ContainsKey(rosterPlayer.Id))
        {
            return false;
        }

        _players[rosterPlayer.Id] = rosterPlayer;

        return true;
    }

    public bool RemovePlayer(ulong id)
    {
        return _players.Remove(id);
    }

    public RosterPlayer GetPlayer(ulong id)
    {
        return _players[id];
    }

    public bool ContainsPlayer(ulong userId)
    {
        return _players.ContainsKey(userId);
    }
}