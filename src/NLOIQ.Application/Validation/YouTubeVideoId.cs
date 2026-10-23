using System.Text.RegularExpressions;
namespace NLOIQ.Application.Validation;
public static partial class YouTubeVideoId
{
    // IDs only. Never accept or forward arbitrary iframe URLs.
    [GeneratedRegex(@"\A[A-Za-z0-9_-]{11}\z", RegexOptions.CultureInvariant)]
    private static partial Regex ValidId();
    public static bool IsValid(string? value) => value is not null && ValidId().IsMatch(value);
}
