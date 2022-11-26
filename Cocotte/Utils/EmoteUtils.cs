using Discord;

namespace Cocotte.Utils;

public static class EmoteUtils
{
    public static IEmote ToEmote(this string emoteText)
    {
        if (Emote.TryParse(emoteText, out var emote))
        {
            return emote;
        }

        return Emoji.Parse(emoteText);
    }
}