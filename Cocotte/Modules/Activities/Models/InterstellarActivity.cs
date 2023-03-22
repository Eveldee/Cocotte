namespace Cocotte.Modules.Activities.Models;

public class InterstellarActivity : Activity
{
    public required InterstellarColor Color { get; init; }

    public override string ToString()
    {
        return $"{base.ToString()}, {nameof(InterstellarColor)}: {Color}";
    }
}