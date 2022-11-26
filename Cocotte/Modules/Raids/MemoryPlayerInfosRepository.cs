using System.Diagnostics.CodeAnalysis;

namespace Cocotte.Modules.Raids;

public class MemoryPlayerInfosRepository : IPlayerInfosRepository
{
    private readonly IDictionary<ulong, PlayerInfo> _playerInfos;

    public MemoryPlayerInfosRepository()
    {
        _playerInfos = new Dictionary<ulong, PlayerInfo>();
    }

    public bool TryGetPlayerInfo(ulong playerId, [MaybeNullWhen(false)] out PlayerInfo playerInfo)
    {
        return _playerInfos.TryGetValue(playerId, out playerInfo);
    }

    public void UpdatePlayerInfo(PlayerInfo playerInfo)
    {
        _playerInfos[playerInfo.Id] = playerInfo;
    }
}