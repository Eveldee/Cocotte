namespace Cocotte.Modules.Raids;

public class RaidRegisterManager
{
    public readonly IDictionary<(ulong raidId, ulong playerId), RosterPlayer> RegisteringPlayers =
        new Dictionary<(ulong raidId, ulong playerId), RosterPlayer>();
}