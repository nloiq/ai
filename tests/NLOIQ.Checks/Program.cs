using NLOIQ.Application.Validation;
string?[] invalid = [null, "", "short", "NNiszX0-A7M\n", "NNiszX0-A7M\r", "https://youtu.be/NNiszX0-A7M", "NNiszX0/A7M", "NNiszX0-A7M?autoplay=1", "<iframe />"];
foreach (var value in invalid)
    if (YouTubeVideoId.IsValid(value)) throw new Exception("Invalid video ID was accepted.");
foreach (var value in new[] { "NNiszX0-A7M", "a_B-cD12345", "___________" })
    if (!YouTubeVideoId.IsValid(value)) throw new Exception("Valid video ID was rejected.");
Console.WriteLine("PASS: server video-ID validation (12 cases)");
