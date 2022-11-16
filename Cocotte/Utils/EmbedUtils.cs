using Discord;

namespace Cocotte.Utils;

public static class EmbedUtils
{
    public static EmbedBuilder ErrorEmbed(string message)
    {
        return new EmbedBuilder()
            .WithColor(Colors.ErrorColor)
            .WithAuthor(a => a
                .WithName("Error")
                .WithIconUrl("https://sage.cdn.ilysix.fr/assets/Cocotte/icons/error.webp")
            )
            .WithDescription(message);
    }

    public static EmbedBuilder InfoEmbed(string message)
    {
        return new EmbedBuilder()
            .WithColor(Colors.InfoColor)
            .WithAuthor(a => a
                .WithName("Info")
                .WithIconUrl("https://sage.cdn.ilysix.fr/assets/Cocotte/icons/info.webp")
            )
            .WithDescription(message);
    }
}