namespace Cocotte.Modules.Activities;

[Flags]
public enum PlayerRoles : byte
{
    None = 0b0000_0000,
    Helper = 0b0000_0001,
    Dps = 0b0000_0010,
    Tank = 0b0000_0100,
    Support = 0b0000_1000,
    Organizer = 0b0001_0000
}