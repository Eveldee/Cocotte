using Cocotte.Modules.Raids;

namespace Cocotte.Utils;

public static class RaidExtensions
{
    public static void AddTestPlayers(this Raid raid)
    {
        raid.AddPlayer("YamaRaja", PlayerRole.Healer, 30000, false);
        raid.AddPlayer("Zaku", PlayerRole.Dps, 40000, false);
        raid.AddPlayer("Juchi", PlayerRole.Tank, 40000, false);
        raid.AddPlayer("Akeno", PlayerRole.Dps, 40000, true);
    }
}