using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Cocotte.Modules.Activities.Models;

[PrimaryKey(nameof(GuildId), nameof(ActivityId), nameof(UserId))]
public class ActivityPlayer
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required ulong UserId { get; init; }

    public required string Name { get; init; }

    public ulong GuildId { get; set; }
    public ulong ActivityId { get; init; }
    public required Activity Activity { get; init; }

    public override string ToString()
    {
        return $"{nameof(UserId)}: {UserId}, {nameof(Name)}: {Name}";
    }
}