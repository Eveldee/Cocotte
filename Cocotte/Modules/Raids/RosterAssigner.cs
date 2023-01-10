using Cocotte.Utils;

namespace Cocotte.Modules.Raids;

public class RosterAssigner
{
    public void AssignRosters(IEnumerable<RosterPlayer> players, uint playersPerRoster)
    {
        // Start by grouping players
        var groups = GroupPlayers(players.OrderByDescending(p => p.Fc));

        // Create rosters
        var neededRosters = (int)Math.Ceiling(players.Count(p => !p.Substitute) / (double)playersPerRoster);
        var rosters = Enumerable.Range(0, neededRosters).Select(_ => new RosterInfo(playersPerRoster)).ToList();

        // Todo Check when there's more than max players per roster

        // First pass: assign healers and tanks
        // Always assign to the group which have the least amount of healer/tank, biased towards healers
        // Skip groups without players
        var dpsGroup = new List<PlayerGroup>();
        foreach (var group in groups.Where(g => !g.AllSubstitutes))
        {
            if (group.Players.AnyHealer())
            {
                var nextHealerRoster = rosters.NonFull(group.Players.Count()).MinBy(r => r.RealHealerCount(), (x, y) => x.RealHealerFc() > y.RealHealerFc() ? y : x);

                nextHealerRoster.AddGroup(group);
            }
            else if (group.Players.AnyTank())
            {
                var nextTankRoster = rosters.NonFull(group.Players.Count()).MinBy(r => r.RealTankCount(), (x, y) => x.RealTankFc() < y.RealTankFc() ? x : y);

                nextTankRoster.AddGroup(group);
            }
            // Those groups will be used to assign dps, they should still be in descending order of FC
            else
            {
                dpsGroup.Add(group);
            }
        }

        // Third pass: assign dps
        foreach (var group in dpsGroup)
        {
            var nextDpsRoster = rosters.NonFull(group.Players.Count()).MinBy(r => r.TotalRealFc);

            nextDpsRoster!.AddGroup(group);
        }

        // Last pass: do the same but with substitutes
        dpsGroup = new List<PlayerGroup>();
        foreach (var group in groups.Where(g => g.AllSubstitutes))
        {
            if (group.Substitutes.AnyHealer())
            {
                var nextHealerRoster = rosters.MinBy(r => r.TotalHealerCount(), (x, y) => x.TotalFc > y.TotalFc ? y : x);

                nextHealerRoster.AddGroup(group);
            }
            else if (group.Substitutes.AnyTank())
            {
                var nextTankRoster = rosters.MinBy(r => r.TotalTankCount(), (x, y) => x.TotalFc < y.TotalFc ? x : y);

                nextTankRoster.AddGroup(group);
            }
            // Those groups will be used to assign dps, they should still be in descending order of FC
            else
            {
                dpsGroup.Add(group);
            }
        }

        // Third pass: assign dps
        foreach (var group in dpsGroup)
        {
            var nextDpsRoster = rosters.MinBy(r => r.TotalFc);

            nextDpsRoster!.AddGroup(group);
        }

        // Assign rosters
        for (int i = 0; i < rosters.Count; i++)
        {
            var roster = rosters[i];

            roster.AssignRosterNumer(i + 1);
        }
    }

    private IList<PlayerGroup> GroupPlayers(IEnumerable<RosterPlayer> players)
    {
        var groups = new List<PlayerGroup>();

        // Todo create groups from player preferences
        foreach (var rosterPlayer in players)
        {
            groups.Add(new PlayerGroup(rosterPlayer));
        }

        return groups;
    }
}

public class RosterInfo
{
    public long TotalRealFc => _groups.Sum(g => g.RealFc);
    public long TotalFc => _groups.Sum(g => g.TotalFc);

    public IEnumerable<PlayerGroup> PlayerGroups => _groups.Where(g => !g.AllSubstitutes);
    public IEnumerable<PlayerGroup> SubstituteGroups => _groups.Where(g => g.AllSubstitutes);
    
    public uint MaxPlayerCount { get; }

    private readonly IList<PlayerGroup> _groups;

    public RosterInfo(uint maxPlayerCount)
    {
        MaxPlayerCount = maxPlayerCount;
        _groups = new List<PlayerGroup>();
    }

    public void AddGroup(PlayerGroup group)
    {
        _groups.Add(group);
    }

    public void AssignRosterNumer(int rosterNumber)
    {
        foreach (var group in _groups)
        {
            group.AssignRosterNumber(rosterNumber);
        }
    }
}

public class PlayerGroup
{
    public long RealFc => _players.Where(p => !p.Substitute).TotalFc();
    public long TotalFc => _players.TotalFc();
    public bool AllSubstitutes => _players.All(p => p.Substitute);

    public IEnumerable<RosterPlayer> Players => _players.Where(p => !p.Substitute);
    public IEnumerable<RosterPlayer> Substitutes => _players.Where(p => p.Substitute);

    private readonly IList<RosterPlayer> _players;

    public PlayerGroup()
    {
        _players = new List<RosterPlayer>();
    }

    public PlayerGroup(params RosterPlayer[] players)
    {
        _players = players;
    }

    public void AddPlayer(RosterPlayer player)
    {
        _players.Add(player);
    }

    public void AssignRosterNumber(int rosterNumber)
    {
        foreach (var rosterPlayer in _players)
        {
            rosterPlayer.RosterNumber = rosterNumber;
        }
    }
}