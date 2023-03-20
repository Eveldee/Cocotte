using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Cocotte.Modules.Activity.Models;

public abstract class Activity
{
    [Key]
    public ulong ActivityId { get; set; }

    public ulong CreatorId { get; init; }
    public string? Description { get; init; }
    public ActivityType ActivityType { get; init; }
    public ActivityName ActivityName { get; init; }
    public uint MaxPlayers { get; set; }

    public List<ActivityPlayer> ActivityPlayers { get; init; } = new();

    public override string ToString()
    {
        return $"{nameof(ActivityId)}: {ActivityId}, {nameof(CreatorId)}: {CreatorId}, {nameof(Description)}: {Description}, {nameof(ActivityType)}: {ActivityType}, {nameof(ActivityName)}: {ActivityName}, {nameof(MaxPlayers)}: {MaxPlayers}";
    }
}