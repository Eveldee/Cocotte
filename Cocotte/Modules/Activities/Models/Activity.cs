using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cocotte.Modules.Activities.Models;

public class Activity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required ulong ActivityId { get; set; }

    public required ulong CreatorDiscordId { get; init; }
    public required string CreatorDiscordName { get; set; }
    public string? Description { get; init; }
    public required ActivityType Type { get; init; }
    public required ActivityName Name { get; init; }
    public required bool AreRolesEnabled { get; init; }
    public required uint MaxPlayers { get; set; }

    public List<ActivityPlayer> ActivityPlayers { get; init; } = new();

    public override string ToString()
    {
        return $"{nameof(ActivityId)}: {ActivityId}, {nameof(CreatorDiscordId)}: {CreatorDiscordId}, {nameof(Description)}: {Description}, {nameof(Type)}: {Type}, {nameof(Name)}: {Name}, {nameof(MaxPlayers)}: {MaxPlayers}";
    }
}