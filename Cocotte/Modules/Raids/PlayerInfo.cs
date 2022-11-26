﻿namespace Cocotte.Modules.Raids;

public class PlayerInfo
{
    public static TimeSpan FcUpdateInterval { get; } = TimeSpan.FromDays(3);

    public ulong Id { get; }

    public uint Fc
    {
        get => _fc;
        set
        {
            _fc = value;
            _lastFcUpdate = DateTime.Today;
        }
    }

    public bool IsFcUpdateRequired => DateTime.Today - _lastFcUpdate > FcUpdateInterval;

    private uint _fc;
    private DateTime _lastFcUpdate;

    public PlayerInfo(ulong id, uint fc)
    {
        Id = id;
        Fc = fc;

        _lastFcUpdate = DateTime.Today;
    }
}