namespace NLOIQ.Domain.Content;
public sealed class PageSection : IRevisionOwned
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public long SectionRevisionId { get; set; }
    public ContentSection Section { get; set; } = null!;
    public int DisplayOrder { get; set; }
}
public sealed class ProjectCategoryAssignment : IRevisionOwned
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public long CategoryRevisionId { get; set; }
    public ProjectCategory Category { get; set; } = null!;
}
public sealed class ProjectMedia : IRevisionOwned
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public long MediaUsageId { get; set; }
    public RevisionMedia MediaUsage { get; set; } = null!;
    public int DisplayOrder { get; set; }
}
public sealed class NavigationItem : IRevisionOwned
{
    public long Id { get; set; }
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public long? ParentItemId { get; set; }
    public NavigationItem? Parent { get; set; }
    public long? TargetContentItemId { get; set; }
    public string? Href { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;
}
public sealed class NavigationTranslation : IRevisionOwned
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public long NavigationItemId { get; set; }
    public NavigationItem NavigationItem { get; set; } = null!;
    public string Culture { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
public sealed class ContactDetail : IRevisionOwned
{
    public long Id { get; set; }
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public ContactKind Kind { get; set; }
    public string Value { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
public sealed class ContactTranslation : IRevisionOwned
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public long ContactDetailId { get; set; }
    public ContactDetail ContactDetail { get; set; } = null!;
    public string Culture { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? LocalizedValue { get; set; }
}
public sealed class BusinessHours : IRevisionOwned
{
    public long Id { get; set; }
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public int DayOfWeek { get; set; }
    public bool IsClosed { get; set; }
    public TimeOnly? OpensAt { get; set; }
    public TimeOnly? ClosesAt { get; set; }
}
public sealed class SocialLink : IRevisionOwned
{
    public long Id { get; set; }
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public string Platform { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
