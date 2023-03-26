namespace Cocotte.Options;

public class ActivityOptions
{
    public const string SectionName = "ActivityOptions";

    public ulong OrganizerRoleId { get; init; }
    public required string OrganizerEmote { get; init; }

    public ulong HelperRoleId { get; init; }
    public required string HelperEmote { get; init; }

    public ulong DpsRoleId { get; init; }
    public required string DpsEmote { get; init; }

    public ulong TankRoleId { get; init; }
    public required string TankEmote { get; init; }

    public ulong SupportRoleId { get; init; }
    public required string SupportEmote { get; init; }
}