namespace Cocotte.Modules.Raids;

public static class RosterExtensions
{
    public static bool AnyHealer(this IEnumerable<RosterPlayer> players)
    {
        return players.Any(p => p.Role == PlayerRole.Healer);
    }

    public static bool AnyTank(this IEnumerable<RosterPlayer> players)
    {
        return players.Any(p => p.Role == PlayerRole.Tank);
    }

    public static int HealerCount(this IEnumerable<RosterPlayer> players)
    {
        return players.Count(p => p.Role == PlayerRole.Healer);
    }

    public static int TankCount(this IEnumerable<RosterPlayer> players)
    {
        return players.Count(p => p.Role == PlayerRole.Tank);
    }

    public static long TotalFc(this IEnumerable<RosterPlayer> players)
    {
        return players.Sum(p => p.Fc);
    }

    public static long HealerFc(this IEnumerable<RosterPlayer> players)
    {
        return players.Where(p => p.Role == PlayerRole.Healer).Sum(p => p.Fc);
    }

    public static long TankFc(this IEnumerable<RosterPlayer> players)
    {
        return players.Where(p => p.Role == PlayerRole.Tank).Sum(p => p.Fc);
    }

    public static long RealHealerFc(this RosterInfo roster)
    {
        return roster.PlayerGroups.Sum(group => group.Players.HealerFc());
    }

    public static long RealTankFc(this RosterInfo roster)
    {
        return roster.PlayerGroups.Sum(group => group.Players.TankFc());
    }

    public static int RealHealerCount(this RosterInfo rosterInfo)
    {
        return rosterInfo.PlayerGroups.Sum(g => g.Players.HealerCount());
    }

    public static int TotalHealerCount(this RosterInfo rosterInfo)
    {
        return rosterInfo.PlayerGroups.Concat(rosterInfo.SubstituteGroups).Sum(g => g.Players.Concat(g.Substitutes).HealerCount());
    }

    public static int RealTankCount(this RosterInfo rosterInfo)
    {
        return rosterInfo.PlayerGroups.Sum(g => g.Players.TankCount());
    }

    public static int TotalTankCount(this RosterInfo rosterInfo)
    {
        return rosterInfo.PlayerGroups.Concat(rosterInfo.SubstituteGroups).Sum(g => g.Players.Concat(g.Substitutes).TankCount());
    }

    public static int TotalPlayerCount(this RosterInfo rosterInfo) =>
        rosterInfo.PlayerGroups.Sum(group => group.Players.Count());

    public static IEnumerable<RosterInfo> NonFull(this IEnumerable<RosterInfo> rosters, int addedPlayersCount) =>
        rosters.Where(roster => roster.TotalPlayerCount() + addedPlayersCount <= roster.MaxPlayerCount);
}