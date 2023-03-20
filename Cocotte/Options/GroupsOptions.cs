namespace Cocotte.Options;

public class GroupsOptions
{
    public const string SectionName = "GroupsOptions";

    public ulong HelperRoleId { get; init; }
    public string HelperEmote { get; init; }

    public ulong DpsRoleId { get; init; }
    public string DpsEmote { get; init; }

    public ulong TankRoleId { get; init; }
    public string TankEmote { get; init; }

    public ulong HealerRoleId { get; init; }
    public string HealerEmote { get; init; }
}