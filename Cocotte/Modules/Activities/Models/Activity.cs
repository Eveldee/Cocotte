using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Cocotte.Modules.Activities.Models;

[PrimaryKey(nameof(GuildId), nameof(ActivityId))]
public class Activity
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required ulong GuildId { get; init; }
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required ulong ActivityId { get; init; }

    public required ulong ThreadId { get; init; }
    public required ulong CreatorUserId { get; init; }
    public required string CreatorDisplayName { get; init; }
    public string? Description { get; set; }
    public required ActivityType Type { get; init; }
    public required ActivityName Name { get; init; }
    public required bool AreRolesEnabled { get; init; }
    public required uint MaxPlayers { get; set; }

    public List<ActivityPlayer> ActivityPlayers { get; init; } = new();

    public override string ToString()
    {
        return $"{nameof(ActivityId)}: {ActivityId}, {nameof(CreatorUserId)}: {CreatorUserId}, {nameof(Description)}: {Description}, {nameof(Type)}: {Type}, {nameof(Name)}: {Name}, {nameof(MaxPlayers)}: {MaxPlayers}";
    }
}