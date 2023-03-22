using Discord;

namespace Cocotte.Utils;

public static class Colors
{
    // Main Cocotte colors
    public static Color CocotteBlue => new(0x3196c8);
    public static Color CocotteRed => new(0xe40808);
    public static Color CocotteOrange => new(0xff6d01);

    // Colors used in embeds
    public static Color ErrorColor => new(0xFB6060);
    public static Color InfoColor => new(0x66D9EF);
    public static Color SuccessColor => new(0x2Ecc71);
    public static Color WarningColor => new(0xf1c40F);
}