using Cocotte.Utils;

namespace Cocotte.Modules.Raids;

public class Raid
{
    public ulong Id { get; }
    public DateTime DateTime { get; }
    public IEnumerable<IGrouping<int, RosterPlayer>> Rosters => _players.Select(p => p.Value).GroupBy(p => p.RosterNumber);

    private readonly IDictionary<ulong, RosterPlayer> _players = new Dictionary<ulong, RosterPlayer>();

    public Raid(ulong id, DateTime dateTime)
    {
        Id = id;
        DateTime = dateTime;
    }

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

    public RosterPlayer GetPlayer(ulong id)
    {
        return _players[id];
    }

    public bool ContainsPlayer(ulong userId)
    {
        return _players.ContainsKey(userId);
    }

    public bool RemovePlayer(ulong id)
    {
        return _players.Remove(id);
    }

    public void AssignRosters(RosterAssigner assigner)
    {
        assigner.AssignRosters(_players.Values, 8);
    }

    public override bool Equals(object? other)
    {
        return other is Raid roster && roster.Id == Id;
    }

    public override int GetHashCode()
    {
        return (int) (Id % int.MaxValue);
    }

    public override string ToString()
    {
        return $"Raid({DateTime})";
    }
}