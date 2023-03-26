using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Cocotte.Modules.Activities.Models;

[PrimaryKey(nameof(GuildId), nameof(ChannelId), nameof(MessageId))]
[Index(nameof(ThreadId))]
public class Activity
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required ulong GuildId { get; init; }
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required ulong ChannelId { get; init; }
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required ulong MessageId { get; init; }

    public required ulong ThreadId { get; init; }
    public required ulong CreatorUserId { get; init; }
    public required string CreatorDisplayName { get; init; }
    public DateTime? DueDateTime { get; init; }
    public string? Description { get; set; }
    public required ActivityType Type { get; init; }
    public required ActivityName Name { get; init; }
    public required bool AreRolesEnabled { get; init; }
    public required uint MaxPlayers { get; set; }

    public DateTime CreationDate { get; init; } = DateTime.Now;
    public bool IsClosed { get; set; }

    public List<ActivityPlayer> ActivityPlayers { get; init; } = new();

    public string JobKey => $"{GuildId}/{ChannelId}/{MessageId}";

    public override string ToString()
    {
        return $"{nameof(MessageId)}: {MessageId}, {nameof(CreatorUserId)}: {CreatorUserId}, {nameof(Description)}: {Description}, {nameof(Type)}: {Type}, {nameof(Name)}: {Name}, {nameof(MaxPlayers)}: {MaxPlayers}";
    }
}