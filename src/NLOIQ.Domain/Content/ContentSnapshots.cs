namespace NLOIQ.Domain.Content;
public sealed class ContentPage : IContentSnapshot
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public ContentKind Kind => ContentKind.Page;
    public string LayoutKey { get; set; } = "engineering-folio";
}
public sealed class ContentSection : IContentSnapshot
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public ContentKind Kind => ContentKind.Section;
    public SectionKind SectionKind { get; set; }
    public string LayoutKey { get; set; } = "default";
    public string Background { get; set; } = "paper";
    public bool IsVisible { get; set; } = true;
    public decimal ParallaxIntensity { get; set; } = 0.5m;
    public string? PrimaryButtonTarget { get; set; }
    public string? SecondaryButtonTarget { get; set; }
}
public sealed class ContentProject : IContentSnapshot
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public ContentKind Kind => ContentKind.Project;
    public string? YouTubeVideoId { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public long? CoverMediaUsageId { get; set; }
    public RevisionMedia? CoverMediaUsage { get; set; }
}
public sealed class ProjectCategory : IContentSnapshot
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public ContentKind Kind => ContentKind.ProjectCategory;

}
public sealed class ContentService : IContentSnapshot
{
    public string CategoryKey { get; set; } = "general";
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public ContentKind Kind => ContentKind.Service;
    public string IconKey { get; set; } = "network";
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;
    public long? MediaUsageId { get; set; }
    public RevisionMedia? MediaUsage { get; set; }
}
public sealed class NavigationSettings : IContentSnapshot
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public ContentKind Kind => ContentKind.Navigation;
    public string Location { get; set; } = "main";
}
public sealed class ContactSettings : IContentSnapshot
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public ContentKind Kind => ContentKind.Contact;
    public string TimeZoneId { get; set; } = "Asia/Baghdad";
}
public sealed class FooterSettings : IContentSnapshot
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public ContentKind Kind => ContentKind.Footer;
    public bool ShowCopyright { get; set; } = true;
}
public sealed class SiteSettings : IContentSnapshot
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public ContentKind Kind => ContentKind.SiteSettings;
    public string SiteName { get; set; } = "NLO IQ";
    public string DefaultCulture { get; set; } = "en";
    public string DefaultTheme { get; set; } = "system";
    public string? PublicBaseUrl { get; set; }
    public long? LogoMediaUsageId { get; set; }
    public RevisionMedia? LogoMediaUsage { get; set; }
    public long? FaviconMediaUsageId { get; set; }
    public RevisionMedia? FaviconMediaUsage { get; set; }
}
public sealed class ThemeSettings : IContentSnapshot
{
    public bool LogoBorderEnabled { get; set; }
    public string LogoBorderColor { get; set; } = "#687C50";
    public int LogoRadius { get; set; } = 8;
    public int LogoHeight { get; set; } = 48;
    public int ThemeSelectorStyle { get; set; }
    public int ThemeSelectorRadius { get; set; } = 8;
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public ContentKind Kind => ContentKind.ThemeSettings;
    public string Evergreen { get; set; } = "#14271F";
    public string Forest { get; set; } = "#203D32";
    public string Moss { get; set; } = "#687C50";
    public string BurntOrange { get; set; } = "#C56A3D";
    public string Copper { get; set; } = "#A65E40";
    public string Gold { get; set; } = "#D8AD64";
    public string Ivory { get; set; } = "#F3EFE5";
    public string Charcoal { get; set; } = "#222722";
    public string LightAction { get; set; } = "#884125";
    public string SignalLight { get; set; } = "#17665F";
    public string SignalDark { get; set; } = "#82C6B8";
}
