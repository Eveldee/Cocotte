using Discord;

namespace Cocotte.Utils;

public static class EmbedUtils
{
    public static EmbedBuilder ErrorEmbed(string message, string title = "Erreur")
    {
        return new EmbedBuilder()
            .WithColor(Colors.ErrorColor)
            .WithAuthor(a => a
                .WithName(title)
                .WithIconUrl(CdnUtils.GetAsset("icons/error.webp"))
            )
            .WithDescription(message);
    }

    public static EmbedBuilder InfoEmbed(string message, string title = "Info")
    {
        return new EmbedBuilder()
            .WithColor(Colors.InfoColor)
            .WithAuthor(a => a
                .WithName(title)
                .WithIconUrl(CdnUtils.GetAsset("icons/info.webp"))
            )
            .WithDescription(message);
    }

    public static EmbedBuilder SuccessEmbed(string message, string title = "Succès")
    {
        return new EmbedBuilder()
            .WithColor(Colors.SuccessColor)
            .WithAuthor(a => a
                .WithName(title)
                .WithIconUrl(CdnUtils.GetAsset("icons/success.webp"))
            )
            .WithDescription(message);
    }

    public static EmbedBuilder WarningEmbed(string message, string title = "Attention")
    {
        return new EmbedBuilder()
            .WithColor(Colors.WarningColor)
            .WithAuthor(a => a
                .WithName(title)
                .WithIconUrl(CdnUtils.GetAsset("icons/warning.webp"))
            )
            .WithDescription(message);
    }
}