namespace NLOIQ.Domain.Content;
public sealed class MediaAsset
{
    public long Id { get; set; }
    public string StorageProvider { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long ByteSize { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public bool IsDemo { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
// Editor suggestions only. Public alt/caption values belong to immutable revision media.
public sealed class MediaAssetTranslation
{
    public long MediaAssetId { get; set; }
    public MediaAsset MediaAsset { get; set; } = null!;
    public string Culture { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
}
public sealed class RevisionMedia : IRevisionOwned
{
    public long Id { get; set; }
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public long MediaAssetId { get; set; }
    public MediaAsset MediaAsset { get; set; } = null!;
    public string UsageKey { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
public sealed class RevisionMediaTranslation : IRevisionOwned
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public long RevisionMediaId { get; set; }
    public RevisionMedia RevisionMedia { get; set; } = null!;
    public string Culture { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public string? Caption { get; set; }
}
public sealed class SeoMetadata : IRevisionOwned
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public string Culture { get; set; } = string.Empty;
    public ContentTranslation Translation { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OpenGraphTitle { get; set; }
    public string? OpenGraphDescription { get; set; }
    public long? OpenGraphMediaUsageId { get; set; }
    public RevisionMedia? OpenGraphMediaUsage { get; set; }
    public string? CanonicalPath { get; set; }
    public bool NoIndex { get; set; } = true;
}
