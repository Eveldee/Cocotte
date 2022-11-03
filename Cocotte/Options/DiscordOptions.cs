namespace Cocotte.Options;

public class DiscordOptions
{
    public const string SectionName = "DiscordOptions";

    public string? Token { get; init; }
    public ulong? DevGuildId { get; init; }
}