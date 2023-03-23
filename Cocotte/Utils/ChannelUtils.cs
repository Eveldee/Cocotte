using Discord;

namespace Cocotte.Utils;

public class ChannelUtils
{
    public static string GetChannelLink(IGuildChannel guildChannel)
    {
        return GetChannelLink(guildChannel.GuildId, guildChannel.Id);
    }

    public static string GetChannelLink(ulong guildId, ulong channelId)
    {
        return $"https://discord.com/channels/{guildId}/{channelId}";
    }
}