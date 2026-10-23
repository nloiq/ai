using System.ComponentModel.DataAnnotations;
using NLOIQ.Domain.Content;
namespace NLOIQ.Application.Content;
public sealed class TranslationEdit
{
    [StringLength(200)] public string Title { get; set; } = "";
    [StringLength(2000)] public string Summary { get; set; } = "";
    [StringLength(16000)] public string Body { get; set; } = "";
    [StringLength(160)] public string? Slug { get; set; }
    [StringLength(120)] public string? PrimaryButtonLabel { get; set; }
    [StringLength(120)] public string? SecondaryButtonLabel { get; set; }
}
public sealed class ContentEdit
{
    public long Id { get; set; }
    public long Version { get; set; }
    public ContentKind Kind { get; set; }
    [Required, RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$"), StringLength(80)] public string Key { get; set; } = "";
    [StringLength(500)] public string ChangeNote { get; set; } = "";
    public TranslationEdit English { get; set; } = new();
    public TranslationEdit Arabic { get; set; } = new();
    public SeoEdit SeoEnglish { get; set; } = new();
    public SeoEdit SeoArabic { get; set; } = new();
    public SiteEdit Site { get; set; } = new();
    public ThemeEdit Theme { get; set; } = new();
    public SectionKind SectionKind { get; set; }
    public string LayoutKey { get; set; } = "default";
    public string Background { get; set; } = "paper";
    public bool IsVisible { get; set; } = true;
    [Range(0,1)] public decimal ParallaxIntensity { get; set; } = 0.5m;
    [StringLength(500)] public string? PrimaryButtonTarget { get; set; }
    [StringLength(500)] public string? SecondaryButtonTarget { get; set; }
    [StringLength(11)] public string? YouTubeVideoId { get; set; }
    public bool IsFeatured { get; set; }
    [Range(0,99999)] public int DisplayOrder { get; set; }
    public string IconKey { get; set; } = "network";
    public bool ShowCopyright { get; set; } = true;
    public List<SectionEdit> Sections { get; set; } = [];
    public List<long> Categories { get; set; } = [];
    public List<MediaEdit> Media { get; set; } = [];
    public List<NavigationEdit> Links { get; set; } = [];
    public List<ContactEdit> Contacts { get; set; } = [];
    public List<SocialEdit> Social { get; set; } = [];
    public List<HoursEdit> Hours { get; set; } = [];
}
public sealed class SectionEdit { public long ContentItemId { get; set; } public int DisplayOrder { get; set; } }
public sealed class MediaEdit
{
    public long AssetId { get; set; }
    [Required,RegularExpression("^[a-z0-9-]+$"),StringLength(40)] public string UsageKey { get; set; } = "cover";
    [StringLength(500)] public string AltEn { get; set; } = "";
    [StringLength(500)] public string AltAr { get; set; } = "";
    [StringLength(1000)] public string? CaptionEn { get; set; }
    [StringLength(1000)] public string? CaptionAr { get; set; }
}
public sealed class NavigationEdit
{
    [StringLength(120)] public string LabelEn { get; set; } = "";
    [StringLength(120)] public string LabelAr { get; set; } = "";
    [StringLength(500)] public string Href { get; set; } = "";
    public int? ParentIndex { get; set; }
    public bool IsVisible { get; set; } = true;
}
public sealed class ContactEdit
{
    public ContactKind Kind { get; set; }
    [Required,StringLength(1000)] public string Value { get; set; } = "";
    [StringLength(120)] public string LabelEn { get; set; } = "";
    [StringLength(120)] public string LabelAr { get; set; } = "";
    [StringLength(1000)] public string? ValueEn { get; set; }
    [StringLength(1000)] public string? ValueAr { get; set; }
}
public sealed class SocialEdit { public string Platform { get; set; } = "linkedin"; [Required,StringLength(1000)] public string Url { get; set; } = ""; }
public sealed class HoursEdit { [Range(0,6)] public int DayOfWeek { get; set; } public bool IsClosed { get; set; } public TimeOnly? OpensAt { get; set; } public TimeOnly? ClosesAt { get; set; } }
public sealed record ContentListRow(long Id, ContentKind Kind, string Key, PublicationState State, bool IsDemo, long Version, int Revision, string Title, SectionKind? SectionKind=null);
public sealed record ContentChoice(long Id, string Label);
public sealed record MediaChoice(long Id, string Label, bool IsDemo,string AltEn="",string AltAr="");
public sealed record EditorChoices(IReadOnlyList<ContentChoice> Sections,IReadOnlyList<ContentChoice> Categories,IReadOnlyList<MediaChoice> Media);
public interface IContentEditor
{
    Task<IReadOnlyList<ContentListRow>> ListAsync(ContentKind? kind, string? search, CancellationToken ct = default);
    Task<ContentEdit?> GetAsync(long id, CancellationToken ct = default);
    Task<EditorChoices> ChoicesAsync(CancellationToken ct = default);
    Task<long> SaveAsync(ContentEdit model, Guid actorId, string actorLabel, CancellationToken ct = default);
    Task TransitionAsync(long id, long version, string action, bool verified, Guid actorId, string actorLabel, CancellationToken ct = default);
}
public sealed class ContentConflictException : Exception { public ContentConflictException() : base("Content changed in another session. Reload before saving.") {} }

public sealed class SeoEdit
{
    [StringLength(200)] public string Title { get; set; }="";
    [StringLength(320)] public string Description { get; set; }="";
    [StringLength(200)] public string? OpenGraphTitle { get; set; }
    [StringLength(320)] public string? OpenGraphDescription { get; set; }
    [StringLength(40)] public string? OpenGraphUsageKey { get; set; }
    [StringLength(500)] public string? CanonicalPath { get; set; }
    public bool NoIndex { get; set; }=true;
}
public sealed class SiteEdit
{
    [Required,StringLength(160)] public string SiteName { get; set; }="NLO IQ";
    public string DefaultCulture { get; set; }="en";
    public string DefaultTheme { get; set; }="system";
    [StringLength(500)] public string? PublicBaseUrl { get; set; }
    public long? LogoAssetId { get; set; }
    public long? FaviconAssetId { get; set; }
    [StringLength(500)] public string LogoAltEn { get; set; }="";
    [StringLength(500)] public string LogoAltAr { get; set; }="";
}
public sealed class ThemeEdit
{
    [Required,RegularExpression("^#[0-9a-fA-F]{6}$")] public string Evergreen { get; set; }="#14271F"; [Required,RegularExpression("^#[0-9a-fA-F]{6}$")] public string Forest { get; set; }="#203D32";
    [Required,RegularExpression("^#[0-9a-fA-F]{6}$")] public string Moss { get; set; }="#687C50";[Required,RegularExpression("^#[0-9a-fA-F]{6}$")] public string BurntOrange { get; set; }="#C56A3D";
    [Required,RegularExpression("^#[0-9a-fA-F]{6}$")] public string Copper { get; set; }="#A65E40";[Required,RegularExpression("^#[0-9a-fA-F]{6}$")] public string Gold { get; set; }="#D8AD64";
    [Required,RegularExpression("^#[0-9a-fA-F]{6}$")] public string Ivory { get; set; }="#F3EFE5";[Required,RegularExpression("^#[0-9a-fA-F]{6}$")] public string Charcoal { get; set; }="#222722";
    [Required,RegularExpression("^#[0-9a-fA-F]{6}$")] public string LightAction { get; set; }="#884125";[Required,RegularExpression("^#[0-9a-fA-F]{6}$")] public string SignalLight { get; set; }="#17665F";[Required,RegularExpression("^#[0-9a-fA-F]{6}$")] public string SignalDark { get; set; }="#82C6B8";
}
