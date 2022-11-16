namespace Cocotte.Modules.Raids;

public class RosterManager
{
    private readonly ISet<RosterPlayer> _rosters = new HashSet<RosterPlayer>();

    public IEnumerable<IGrouping<int, RosterPlayer>> Rosters => _rosters.GroupBy(p => p.RosterNumber);

    public bool AddPlayer(RosterPlayer rosterPlayer)
    {
        // TODO add logic to split player in multiple rosters
        rosterPlayer.RosterNumber = 1;

        return _rosters.Add(rosterPlayer);
    }
}