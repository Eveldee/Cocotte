using Cocotte.Modules.Activities.Models;
using Cocotte.Utils;

namespace Cocotte.Modules.Activities;

public class InterstellarFormatter
{
    public string FormatInterstellarColor(InterstellarColor color) => color switch
    {
        InterstellarColor.Black => "noire",
        InterstellarColor.Blue => "bleue",
        InterstellarColor.Green => "verte",
        InterstellarColor.Red => "rouge",
        _ => "N/A"
    };

    public string InterstellarColorEmote(InterstellarColor color) => color switch
    {
        InterstellarColor.Black => "<:gate_black:1088385899022258248>",
        InterstellarColor.Blue => "<:gate_blue:1088385901324935200>",
        InterstellarColor.Green => "<:gate_green:1088385902792945664>",
        InterstellarColor.Red => "<:gate_red:1088385905405984859>",
        _ => ""
    };

    public string GetColorIcon(InterstellarColor color)
    {
        return CdnUtils.GetAsset($"icons/tof/gate/gate_{color.ToString().ToLowerInvariant()}.webp");
    }
}