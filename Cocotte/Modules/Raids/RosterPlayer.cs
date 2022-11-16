namespace Cocotte.Modules.Raids;

public record RosterPlayer(string Name, PlayerRole Role, int Fc, bool Substitue = false)
{
    public int RosterNumber { get; set; }

    private static string RoleToEmote(PlayerRole role) => role switch
    {
        PlayerRole.Dps => ":red_circle:",
        PlayerRole.Tank => ":yellow_circle:",
        PlayerRole.Healer => ":green_circle:",
        _ => ":question:"
    };

    public static string FcFormat(int fc) => fc switch
    {
        < 1_000 => $"{fc}",
        _ => $"{fc/1000}k"
    };

    public override string ToString() => Substitue switch
    {
        false => $"{RoleToEmote(Role)} {Name} ({FcFormat(Fc)} FC)",
        true => $"*{RoleToEmote(Role)} {Name} ({FcFormat(Fc)} FC)*"
    };
}

public enum PlayerRole
{
    Dps,
    Healer,
    Tank
}