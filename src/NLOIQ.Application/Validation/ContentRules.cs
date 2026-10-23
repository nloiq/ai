using System.Globalization;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using NLOIQ.Domain.Content;
namespace NLOIQ.Application.Validation;
public static partial class ContentRules
{
    [GeneratedRegex(@"\A#[0-9a-fA-F]{6}\z")]
    private static partial Regex HexColor();
    [GeneratedRegex(@"\A[\p{L}\p{N}]+(?:-[\p{L}\p{N}]+)*\z")]
    private static partial Regex SlugPattern();
    [GeneratedRegex(@"\A[+0-9() .-]{3,40}\z")]
    private static partial Regex PhonePattern();
    public static bool IsCulture(string value) => value is "en" or "ar";
    public static bool IsSafeTarget(string? value)
    {
        if (string.IsNullOrEmpty(value)) return true;
        if (value.Any(char.IsControl) || value.Contains('\\')) return false;
        if (value.StartsWith('#')) return SlugPattern().IsMatch(value[1..]);
        if (value.StartsWith('/')) return !value.StartsWith("//", StringComparison.Ordinal);
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps && string.IsNullOrEmpty(uri.UserInfo);
    }
    public static void Validate(object value)
    {
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        switch (value)
        {
            case ContentItem item:
                Require(Enum.IsDefined(item.Kind) && Enum.IsDefined(item.State), "Unknown content kind or state.");
                Require(!string.IsNullOrWhiteSpace(item.Key), "Content key is required."); break;
            case ContentTranslation translation:
                Require(IsCulture(translation.Culture), "Unsupported content culture.");
                Require(translation.Title.Length <= 200 && translation.Summary.Length <= 2000 && translation.Body.Length <= 16000, "Content translation exceeds field limits.");
                Require(translation.Slug is null || (SlugPattern().IsMatch(translation.Slug) && translation.Slug == translation.Slug.ToLowerInvariant().Normalize(NormalizationForm.FormC)), "Invalid localized slug."); break;
            case ContentSection section:
                Require(Enum.IsDefined(section.SectionKind), "Unknown section type.");
                Require(section.ParallaxIntensity is >= 0 and <= 1, "Parallax intensity must be between zero and one.");
                Require(section.LayoutKey is "default" or "folio" or "wide" or "numbered", "Unapproved section layout.");
                Require(section.Background is "paper" or "panel" or "evergreen", "Unapproved section background.");
                Require(IsSafeTarget(section.PrimaryButtonTarget) && IsSafeTarget(section.SecondaryButtonTarget), "Unsafe section link."); break;
            case ContentPage page: Require(page.LayoutKey is "engineering-folio", "Unapproved page layout."); break;
            case ContentProject project:
                Require(project.YouTubeVideoId is null || YouTubeVideoId.IsValid(project.YouTubeVideoId), "Invalid project video ID.");
                Require(project.DisplayOrder >= 0, "Project order cannot be negative."); break;
            case ContentService service:
                Require(service.IconKey is "network" or "code" or "monitor" or "logic", "Unapproved service icon.");
                Require(service.DisplayOrder >= 0, "Service order cannot be negative."); break;
            case NavigationItem navigation: Require(IsSafeTarget(navigation.Href) && navigation.DisplayOrder >= 0, "Invalid navigation target/order."); break;
            case SocialLink social:
                Require(social.Platform is "linkedin" or "facebook" or "instagram" or "youtube" or "x", "Unsupported social platform.");
                Require(Uri.TryCreate(social.Url, UriKind.Absolute, out var socialUri) && socialUri.Scheme == Uri.UriSchemeHttps && IsSafeTarget(social.Url), "Social links require HTTPS."); break;
            case ContactDetail detail:
                Require(Enum.IsDefined(detail.Kind) && !string.IsNullOrWhiteSpace(detail.Value), "Invalid contact detail.");
                Require(detail.Kind switch {
                    ContactKind.Email => MailAddress.TryCreate(detail.Value, out var mail) && mail.Address == detail.Value,
                    ContactKind.Phone or ContactKind.WhatsApp => PhonePattern().IsMatch(detail.Value),
                    ContactKind.Map => Uri.TryCreate(detail.Value, UriKind.Absolute, out var map) && map.Scheme == Uri.UriSchemeHttps && IsSafeTarget(detail.Value),
                    _ => !detail.Value.Any(char.IsControl)
                }, "Invalid contact value."); break;
            case SiteSettings settings:
                Require(IsCulture(settings.DefaultCulture) && settings.DefaultTheme is "light" or "dark" or "system", "Invalid site defaults.");
                Require(settings.PublicBaseUrl is null || (Uri.TryCreate(settings.PublicBaseUrl, UriKind.Absolute, out var siteUri) && siteUri.Scheme == Uri.UriSchemeHttps && IsSafeTarget(settings.PublicBaseUrl)), "Invalid public base URL."); break;
            case ThemeSettings theme:
                foreach (var property in typeof(ThemeSettings).GetProperties().Where(x => x.PropertyType == typeof(string)))
                    Require(HexColor().IsMatch((string)property.GetValue(theme)!), "Theme colors require six-digit hex values.");
                Require(Contrast(theme.Evergreen, theme.Ivory) >= 4.5 && Contrast(theme.LightAction, theme.Ivory) >= 4.5 && Contrast(theme.SignalLight, theme.Ivory) >= 4.5 && Contrast(theme.SignalDark, theme.Charcoal) >= 4.5 && Contrast(theme.Gold, theme.Evergreen) >= 4.5, "Theme customization fails approved text contrast pairs."); break;
            case AuditLog audit when audit.DetailsJson is not null:
                using (JsonDocument.Parse(audit.DetailsJson)) { } break;
        }
    }
    public static void RequirePublishable(ContentKind kind, IReadOnlyCollection<ContentTranslation> translations)
    {
        foreach (var culture in new[] { "en", "ar" })
        {
            var translation = translations.SingleOrDefault(x => x.Culture == culture);
            if (translation is null || string.IsNullOrWhiteSpace(translation.Title)) throw new InvalidOperationException("Both English and Arabic titles are required before publication.");
            if (kind is ContentKind.Page or ContentKind.Project && string.IsNullOrWhiteSpace(translation.Slug)) throw new InvalidOperationException("Localized page/project slugs are required before publication.");
            Validate(translation);
        }
    }
    public static double Contrast(string foreground, string background)
    {
        static double Luminance(string color)
        {
            var rgb = new[] { 1, 3, 5 }.Select(index => int.Parse(color.AsSpan(index, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d)
                .Select(channel => channel <= 0.04045 ? channel / 12.92 : Math.Pow((channel + 0.055) / 1.055, 2.4)).ToArray();
            return rgb[0] * 0.2126 + rgb[1] * 0.7152 + rgb[2] * 0.0722;
        }
        var first = Luminance(foreground); var second = Luminance(background);
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }
}
