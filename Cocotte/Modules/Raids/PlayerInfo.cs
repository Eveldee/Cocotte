namespace Cocotte.Modules.Raids;

public class PlayerInfo
{
    public static TimeSpan FcUpdateInterval { get; } = TimeSpan.FromDays(3);

    public ulong Id { get; }

    private readonly uint _fc;
    public uint Fc
    {
        get => _fc;
        init
        {
            _fc = value;
            _lastFcUpdate = DateTime.Today;
        }
    }

    public bool IsFcUpdateRequired => DateTime.Today - _lastFcUpdate > FcUpdateInterval;

    private readonly DateTime _lastFcUpdate;

    public PlayerInfo(ulong id, uint fc)
    {
        Id = id;
        Fc = fc;

        _lastFcUpdate = DateTime.Today;
    }
}