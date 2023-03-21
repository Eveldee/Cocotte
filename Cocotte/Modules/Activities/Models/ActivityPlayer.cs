using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Cocotte.Modules.Activities.Models;

[PrimaryKey(nameof(ActivityId), nameof(DiscordId))]
public class ActivityPlayer
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required ulong DiscordId { get; init; }

    public required string Name { get; init; }

    public ulong ActivityId { get; init; }
    public required Activity Activity { get; init; }

    public override string ToString()
    {
        return $"{nameof(DiscordId)}: {DiscordId}, {nameof(Name)}: {Name}";
    }
}