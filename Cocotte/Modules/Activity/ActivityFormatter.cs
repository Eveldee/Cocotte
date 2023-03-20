namespace Cocotte.Modules.Activity;

public class ActivityFormatter
{
    public static string ActivityName(ActivityName activityName)
    {
        return activityName switch
        {
            Modules.Activity.ActivityName.Abyss => "Abîme du Néant",
            Modules.Activity.ActivityName.Raids => "Raids",
            Modules.Activity.ActivityName.FrontierClash => "Clash Frontalier",
            Modules.Activity.ActivityName.VoidRift => "Failles du Néant",
            Modules.Activity.ActivityName.OriginsOfWar => "Origines de la Guerre",
            Modules.Activity.ActivityName.JointOperation => "Opération Conjointe",
            Modules.Activity.ActivityName.InterstellarExploration => "Exploration Interstellaire",
            Modules.Activity.ActivityName.BreakFromDestiny => "Échapper au Destin (3v3)",
            Modules.Activity.ActivityName.CriticalAbyss => "Abîme Critique (8v8)",
            Modules.Activity.ActivityName.Event => "Event",
            Modules.Activity.ActivityName.Fishing => "Pêche",
            Modules.Activity.ActivityName.MirroriaRace => "Course Mirroria",
            _ => throw new ArgumentOutOfRangeException(nameof(activityName), activityName, null)
        };
    }
}