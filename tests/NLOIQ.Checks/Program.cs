using NLOIQ.Domain.Content;
using NLOIQ.Application.Validation;
string?[] invalid = [null, "", "short", "NNiszX0-A7M\n", "NNiszX0-A7M\r", "https://youtu.be/NNiszX0-A7M", "NNiszX0/A7M", "NNiszX0-A7M?autoplay=1", "<iframe />"];
foreach (var value in invalid)
    if (YouTubeVideoId.IsValid(value)) throw new Exception("Invalid video ID was accepted.");
foreach (var value in new[] { "NNiszX0-A7M", "a_B-cD12345", "___________" })
    if (!YouTubeVideoId.IsValid(value)) throw new Exception("Valid video ID was rejected.");
Console.WriteLine("PASS: server video-ID validation (12 cases)");


static void Invalid(Action action)
{
    try { action(); } catch (InvalidOperationException) { return; }
    throw new Exception("Invalid content was accepted.");
}
ContentRules.Validate(new ThemeSettings());
Invalid(() => ContentRules.Validate(new ThemeSettings { Ivory = "#14271F" }));
Invalid(() => ContentRules.Validate(new ThemeSettings { Copper = "red;display:none" }));
Invalid(() => ContentRules.Validate(new ContentSection { ParallaxIntensity = 2m }));
Invalid(() => ContentRules.Validate(new ContentService { IconKey = "arbitrary-svg" }));
Invalid(() => ContentRules.Validate(new ContentProject { YouTubeVideoId = "<iframe>" }));
Invalid(() => ContentRules.Validate(new ContentTranslation { Culture = "fr" }));
Invalid(() => ContentRules.Validate(new ContentTranslation { Culture = "en", Slug = "../admin" }));
Invalid(() => ContentRules.Validate(new ContentTranslation { Culture = "en", Body = new string('x', 16001) }));
foreach (var target in new[] { "javascript:alert(1)", "//evil.example", "/\\evil.example", "data:text/html,test", "http://example.com" })
    if (ContentRules.IsSafeTarget(target)) throw new Exception("Unsafe link accepted.");
foreach (var target in new[] { "#about", "/en/", "https://example.com/path" })
    if (!ContentRules.IsSafeTarget(target)) throw new Exception("Safe link rejected.");
ContentRules.RequirePublishable(ContentKind.Service, [new() { Culture="en", Title="Engineering" }, new() { Culture="ar", Title="الهندسة" }]);
Console.WriteLine("PASS: content cultures, limits, links, icons, motion, video and theme contrast checks");
