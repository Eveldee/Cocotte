namespace Cocotte.Modules.Activity.Models;

public class ActivityRolePlayer : ActivityPlayer
{
    public required ActivityRoles Roles { get; init; }
}