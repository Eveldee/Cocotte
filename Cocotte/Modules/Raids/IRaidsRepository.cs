using System.Diagnostics.CodeAnalysis;

namespace Cocotte.Modules.Raids;

public interface IRaidsRepository
{
    Raid this[ulong raidId] { get; }

    bool AddNewRaid(ulong raidId, DateTime dateTime);

    bool TryGetRaid(ulong raidId, [MaybeNullWhen(false)]  out Raid raid);
}