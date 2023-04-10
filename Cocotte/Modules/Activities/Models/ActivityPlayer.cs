using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Cocotte.Modules.Activities.Models;

[PrimaryKey(nameof(GuildId), nameof(ChannelId), nameof(MessageId), nameof(UserId))]
public class ActivityPlayer
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required ulong UserId { get; init; }

    public required string Name { get; init; }
    public bool IsOrganizer { get; init; }
    public bool HasCompleted { get; set; }

    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong MessageId { get; init; }
    public required Activity Activity { get; init; }

    public override string ToString()
    {
        return $"{nameof(UserId)}: {UserId}, {nameof(Name)}: {Name}";
    }
}