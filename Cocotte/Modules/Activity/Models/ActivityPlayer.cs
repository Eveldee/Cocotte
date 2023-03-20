using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Cocotte.Modules.Activity.Models;

[PrimaryKey(nameof(ActivityId), nameof(UserId))]
public class ActivityPlayer
{
    public ulong UserId { get; init; }

    public required string PlayerName { get; init; }

    public ulong ActivityId { get; init; }
    public required Activity Activity { get; init; }

    public override string ToString()
    {
        return $"{nameof(UserId)}: {UserId}, {nameof(PlayerName)}: {PlayerName}";
    }
}