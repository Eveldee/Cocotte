namespace Cocotte.Options;

public class CompositeRolesOptions
{
    public const string SectionName = "CompositeRolesOptions";

    public required IReadOnlyDictionary<string, GuildCompositeRoles[]> CompositeRoles { get; init; }
}

public class GuildCompositeRoles
{
    public required ulong TargetRoleId { get; init; }
    public required ulong[] CompositeRolesIds { get; init; }
}
