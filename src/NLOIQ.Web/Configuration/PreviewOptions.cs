using NLOIQ.Application.Validation;
namespace NLOIQ.Web.Configuration;
public sealed class PreviewOptions
{
    public Dictionary<string, string?> VideoIds { get; set; } = [];
    public bool IsValid() => VideoIds.All(pair =>
        pair.Key is "network" or "scada" or "automation" &&
        (string.IsNullOrEmpty(pair.Value) || YouTubeVideoId.IsValid(pair.Value)));
}
