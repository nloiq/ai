using NLOIQ.Domain.Content;
using NLOIQ.Application.Validation;
// Generate a one-day, untrusted loopback certificate for the isolated release smoke test only.
if(args.Length==2&&args[0]=="--probe-certificate") {
    var destination=Path.GetFullPath(args[1]);var allowed=Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,"artifacts","phase11"))+Path.DirectorySeparatorChar;
    if(!destination.StartsWith(allowed,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Certificate must stay in the ignored quality artifacts directory.");
    var password=Environment.GetEnvironmentVariable("NLO_PROBE_CERT_PASSWORD")??throw new InvalidOperationException("Probe certificate password is required.");
    using var key=System.Security.Cryptography.RSA.Create(2048);
    var request=new System.Security.Cryptography.X509Certificates.CertificateRequest("CN=localhost",key,System.Security.Cryptography.HashAlgorithmName.SHA256,System.Security.Cryptography.RSASignaturePadding.Pkcs1);
    var names=new System.Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder();names.AddDnsName("localhost");names.AddIpAddress(System.Net.IPAddress.Loopback);request.CertificateExtensions.Add(names.Build());
    using var certificate=request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5),DateTimeOffset.UtcNow.AddDays(1));Directory.CreateDirectory(Path.GetDirectoryName(destination)!);File.WriteAllBytes(destination,certificate.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Pfx,password));return;
}
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
