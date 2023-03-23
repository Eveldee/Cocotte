namespace Cocotte.Utils;

public class CdnUtils
{
    /// <summary>
    /// Use this to force discord media cache to fetch new content from CDN
    /// </summary>
    private const string QuerySuffix = $"?q={RandomSuffix}";

    /// <summary>
    /// Needs to be updated each time a media is updated on the CDN
    /// </summary>
    private const string RandomSuffix = "a57z45a";

    public static string GetAsset(string assetName)
    {
        return $"https://sage.cdn.ilysix.fr/assets/Cocotte/{assetName}{QuerySuffix}";
    }
}