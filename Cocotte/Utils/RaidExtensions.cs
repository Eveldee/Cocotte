using Cocotte.Modules.Raids;

namespace Cocotte.Utils;

public static class RaidExtensions
{
    public static void AddTestPlayers(this Raid raid)
    {
        raid.AddPlayer(new RosterPlayer(0, "YamaRaja", PlayerRole.Healer, 30000, false));
        raid.AddPlayer(new RosterPlayer(1, "Zaku", PlayerRole.Dps, 40000, false));
        raid.AddPlayer(new RosterPlayer(2, "Juchi", PlayerRole.Tank, 40000, false));
        raid.AddPlayer(new RosterPlayer(3, "Akeno", PlayerRole.Dps, 40000, true));
    }
}