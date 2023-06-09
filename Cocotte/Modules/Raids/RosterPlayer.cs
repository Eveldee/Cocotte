﻿namespace Cocotte.Modules.Raids;

public record RosterPlayer(ulong Id, string Name, PlayerRole Role = PlayerRole.Dps, uint Fc = 0, bool Substitute = false)
{
    public int RosterNumber { get; set; }

    public override int GetHashCode()
    {
        return (int) (Id % int.MaxValue);
    }

    public virtual bool Equals(RosterPlayer? other)
    {
        return other is not null && other.Id == Id;
    }
}