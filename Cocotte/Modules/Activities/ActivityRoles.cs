namespace Cocotte.Modules.Activities;

[Flags]
public enum ActivityRoles : byte
{
    None = 0b0000,
    Helper = 0b0001,
    Dps = 0b0010,
    Tank = 0b0100,
    Support = 0b1000
}