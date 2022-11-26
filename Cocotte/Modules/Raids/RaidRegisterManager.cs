namespace Cocotte.Modules.Raids;

public class RaidRegisterManager
{
    public IDictionary<(ulong raidId, ulong playerId), RosterPlayer> RegisteringPlayers =
        new Dictionary<(ulong raidId, ulong playerId), RosterPlayer>();
}