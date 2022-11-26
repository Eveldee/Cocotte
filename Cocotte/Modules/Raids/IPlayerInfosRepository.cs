using System.Diagnostics.CodeAnalysis;

namespace Cocotte.Modules.Raids;

public interface IPlayerInfosRepository
{
    bool TryGetPlayerInfo(ulong playerId, [MaybeNullWhen(false)] out PlayerInfo playerInfo);
    void UpdatePlayerInfo(ulong id, Action<PlayerInfo> updater);
}