namespace Cocotte.Modules.Activity;

public record ActivityRolePlayer(ulong UserId, string PlayerName, ActivityRoles Roles) : ActivityPlayer(UserId, PlayerName);