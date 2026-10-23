namespace NLOIQ.Domain.Entities;
// Development preview content only. Full revision-backed CMS aggregates arrive in Phase 5.
public sealed class ServicePreview
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsDemo { get; set; } = true;
    public List<ServicePreviewTranslation> Translations { get; set; } = [];
}
public sealed class ServicePreviewTranslation
{
    public int Id { get; set; }
    public int ServicePreviewId { get; set; }
    public ServicePreview ServicePreview { get; set; } = null!;
    public string Culture { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}
