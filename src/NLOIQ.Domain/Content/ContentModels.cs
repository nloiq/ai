namespace NLOIQ.Domain.Content;
public enum ContentKind { Page, Section, Project, ProjectCategory, Service, Navigation, Contact, Footer, SiteSettings, ThemeSettings }
public enum PublicationState { Draft, Published, Archived }
public enum SectionKind { Hero, About, Projects, Services, Contact, Footer }
public enum ContactKind { Email, Phone, WhatsApp, Address, Map }
public enum ScheduleState { Pending, Completed, Cancelled, Failed }
public interface IRevisionOwned
{
    long RevisionId { get; set; }
    ContentRevision Revision { get; set; }
}
public interface IContentSnapshot : IRevisionOwned { ContentKind Kind { get; } }
public sealed class ContentItem
{
    public long Id { get; set; }
    public ContentKind Kind { get; set; }
    public string Key { get; set; } = string.Empty;
    public PublicationState State { get; set; } = PublicationState.Draft;
    public bool IsDemo { get; set; }
    public long Version { get; set; } = 1;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public long? DraftRevisionId { get; set; }
    public ContentRevision? DraftRevision { get; set; }
    public long? PublishedRevisionId { get; set; }
    public ContentRevision? PublishedRevision { get; set; }
}
public sealed class ContentRevision
{
    public long Id { get; set; }
    public long ContentItemId { get; set; }
    public ContentItem ContentItem { get; set; } = null!;
    public int RevisionNumber { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public Guid? CreatedByUserId { get; set; }
    public long? SupersedesRevisionId { get; set; }
    public string ChangeNote { get; set; } = string.Empty;
    public List<ContentTranslation> Translations { get; set; } = [];
}
public sealed class ContentTranslation : IRevisionOwned
{
    public long RevisionId { get; set; }
    public ContentRevision Revision { get; set; } = null!;
    public string Culture { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? PrimaryButtonLabel { get; set; }
    public string? SecondaryButtonLabel { get; set; }
}
public sealed class PublishedRoute
{
    public long Id { get; set; }
    public string Culture { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public long ContentItemId { get; set; }
    public long RevisionId { get; set; }
}
public sealed class PublicationSchedule
{
    public Guid? RequestedByUserId { get; set; }
    public string RequestedByLabel { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public long Id { get; set; }
    public long ContentItemId { get; set; }
    public long RevisionId { get; set; }
    public DateTime ScheduledUtc { get; set; }
    public ScheduleState State { get; set; }
    public DateTime? CompletedUtc { get; set; }
    public long Version { get; set; } = 1;
    public string? FailureCode { get; set; }
}
public sealed class AuditLog
{
    public long Id { get; set; }
    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
    public Guid? ActorUserId { get; set; }
    public string ActorLabel { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public long? ContentItemId { get; set; }
    public long? RevisionId { get; set; }
    public string? DetailsJson { get; set; }
}
