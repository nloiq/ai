using Microsoft.EntityFrameworkCore;
using NLOIQ.Application.Validation;
using NLOIQ.Domain.Content;
namespace NLOIQ.Infrastructure.Data;
internal static class ContentWriteGuard
{
    public static void Validate(NloDbContext database)
    {
        database.ChangeTracker.DetectChanges();
        var newRevisions = database.ChangeTracker.Entries<ContentRevision>().Where(x => x.State == EntityState.Added).Select(x => x.Entity).ToHashSet();
        foreach (var entry in database.ChangeTracker.Entries().Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (entry.Entity is ContentRevision or IRevisionOwned or AuditLog)
                if (entry.State is EntityState.Modified or EntityState.Deleted) throw new InvalidOperationException("Saved revisions, revision-owned records and audit logs are append-only.");
            if (entry.Entity is IRevisionOwned owned && !newRevisions.Contains(owned.Revision))
                throw new InvalidOperationException("Revision-owned records must be created with a new revision; existing revisions cannot be extended.");
            if (entry.Entity is IContentSnapshot snapshot && snapshot.Revision.ContentItem.Kind != snapshot.Kind)
                throw new InvalidOperationException("Snapshot kind does not match its content identity.");
            if (entry.Entity is MediaAsset && entry.State == EntityState.Modified)
                throw new InvalidOperationException("Media files/metadata are immutable; replacement requires a new asset.");
            if (entry.State is EntityState.Added or EntityState.Modified) ContentRules.Validate(entry.Entity);
            if (entry.Entity is ContentItem or PublicationSchedule && entry.State == EntityState.Modified)
                entry.Property("Version").CurrentValue = checked((long)entry.Property("Version").OriginalValue! + 1);
        }
    }
    public static async Task ValidatePublicationAsync(NloDbContext database, CancellationToken cancellationToken)
    {
        foreach (var entry in database.ChangeTracker.Entries<ContentItem>().Where(x => x.Entity.State == PublicationState.Published &&
                     (x.State == EntityState.Added || (x.State == EntityState.Modified && (x.Property(item => item.State).IsModified || x.Property(item => item.PublishedRevisionId).IsModified)))))
        {
            var item = entry.Entity;
            if (item.IsDemo) throw new InvalidOperationException("Demo content cannot be published. Verify and replace sample material first.");
            if (item.PublishedRevisionId is not long revisionId) throw new InvalidOperationException("Published content requires a published revision pointer.");
            var revision = await database.ContentRevisions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == revisionId && x.ContentItemId == item.Id, cancellationToken)
                ?? throw new InvalidOperationException("Published revision must belong to its content item.");
            var translations = await database.ContentTranslations.AsNoTracking().Where(x => x.RevisionId == revisionId).ToListAsync(cancellationToken);
            ContentRules.RequirePublishable(item.Kind, translations);
            var hasSnapshot = item.Kind switch {
                ContentKind.Page => await database.Set<ContentPage>().AnyAsync(x => x.RevisionId == revision.Id, cancellationToken),
                ContentKind.Section => await database.Set<ContentSection>().AnyAsync(x => x.RevisionId == revision.Id, cancellationToken),
                ContentKind.Project => await database.Set<ContentProject>().AnyAsync(x => x.RevisionId == revision.Id, cancellationToken),
                ContentKind.ProjectCategory => await database.Set<ProjectCategory>().AnyAsync(x => x.RevisionId == revision.Id, cancellationToken),
                ContentKind.Service => await database.Set<ContentService>().AnyAsync(x => x.RevisionId == revision.Id, cancellationToken),
                ContentKind.Navigation => await database.Set<NavigationSettings>().AnyAsync(x => x.RevisionId == revision.Id, cancellationToken),
                ContentKind.Contact => await database.Set<ContactSettings>().AnyAsync(x => x.RevisionId == revision.Id, cancellationToken),
                ContentKind.Footer => await database.Set<FooterSettings>().AnyAsync(x => x.RevisionId == revision.Id, cancellationToken),
                ContentKind.SiteSettings => await database.Set<SiteSettings>().AnyAsync(x => x.RevisionId == revision.Id, cancellationToken),
                ContentKind.ThemeSettings => await database.Set<ThemeSettings>().AnyAsync(x => x.RevisionId == revision.Id, cancellationToken),
                _ => false
            };
            if (!hasSnapshot) throw new InvalidOperationException("Published revision is missing its typed content snapshot.");
            var missingAlt = await database.Set<RevisionMedia>().AnyAsync(media => media.RevisionId == revisionId &&
                (!database.Set<RevisionMediaTranslation>().Any(text => text.RevisionMediaId == media.Id && text.Culture == "en" && text.AltText.Trim() != "") ||
                 !database.Set<RevisionMediaTranslation>().Any(text => text.RevisionMediaId == media.Id && text.Culture == "ar" && text.AltText.Trim() != "")), cancellationToken);
            if (await database.Set<RevisionMedia>().AnyAsync(media => media.RevisionId == revisionId && media.MediaAsset.IsDemo, cancellationToken)) throw new InvalidOperationException("Demo media cannot be published.");
            if (missingAlt) throw new InvalidOperationException("Published media requires English and Arabic alt text snapshots.");
            if (item.Kind == ContentKind.Page && await database.Set<PageSection>().AnyAsync(link => link.RevisionId == revisionId &&
                !database.ContentRevisions.Any(target => target.Id == link.SectionRevisionId && database.ContentItems.Any(owner => owner.Id == target.ContentItemId && owner.State == PublicationState.Published && !owner.IsDemo && owner.PublishedRevisionId == target.Id)), cancellationToken))
                throw new InvalidOperationException("Page sections must reference published, verified section revisions before publication.");
            if (item.Kind == ContentKind.Project && await database.Set<ProjectCategoryAssignment>().AnyAsync(link => link.RevisionId == revisionId &&
                !database.ContentRevisions.Any(target => target.Id == link.CategoryRevisionId && database.ContentItems.Any(owner => owner.Id == target.ContentItemId && owner.State == PublicationState.Published && !owner.IsDemo && owner.PublishedRevisionId == target.Id)), cancellationToken))
                throw new InvalidOperationException("Project categories must be published before the project.");
        }
    }
}
