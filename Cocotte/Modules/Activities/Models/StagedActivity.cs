﻿namespace Cocotte.Modules.Activities.Models;

public class StagedActivity : Activity
{
    public uint Stage { get; set; }

    public override string ToString()
    {
        return $"{base.ToString()}, {nameof(Stage)}: {Stage}";
    }
}