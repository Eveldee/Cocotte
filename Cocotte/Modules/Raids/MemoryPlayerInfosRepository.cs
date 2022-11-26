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

    public void UpdatePlayerInfo(ulong id, Action<PlayerInfo> updater)
    {
        if (_playerInfos.TryGetValue(id, out var playerInfo))
        {
            updater(playerInfo);
        }
        else
        {
            playerInfo = new PlayerInfo(id, 0);
            updater(playerInfo);

            _playerInfos[playerInfo.Id] = playerInfo;
        }
    }
}