using Discord;

namespace Cocotte.Utils;

public static class EmbedUtils
{
    public static EmbedBuilder ErrorEmbed(string message, string title = "Error")
    {
        return new EmbedBuilder()
            .WithColor(Colors.ErrorColor)
            .WithAuthor(a => a
                .WithName(title)
                .WithIconUrl("https://sage.cdn.ilysix.fr/assets/Cocotte/icons/error.webp")
            )
            .WithDescription(message);
    }

    public static EmbedBuilder InfoEmbed(string message, string title = "Info")
    {
        return new EmbedBuilder()
            .WithColor(Colors.InfoColor)
            .WithAuthor(a => a
                .WithName(title)
                .WithIconUrl("https://sage.cdn.ilysix.fr/assets/Cocotte/icons/info.webp")
            )
            .WithDescription(message);
    }
    
    public static EmbedBuilder SuccessEmbed(string message, string title = "Success")
    {
        return new EmbedBuilder()
            .WithColor(Colors.SuccessColor)
            .WithAuthor(a => a
                .WithName(title)
                .WithIconUrl("https://sage.cdn.ilysix.fr/assets/Cocotte/icons/success.webp")
            )
            .WithDescription(message);
    }
    
    public static EmbedBuilder WarningEmbed(string message, string title = "Warning")
    {
        return new EmbedBuilder()
            .WithColor(Colors.WarningColor)
            .WithAuthor(a => a
                .WithName(title)
                .WithIconUrl("https://sage.cdn.ilysix.fr/assets/Cocotte/icons/warning.webp")
            )
            .WithDescription(message);
    }
}