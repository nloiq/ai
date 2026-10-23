using Microsoft.EntityFrameworkCore;
using NLOIQ.Application.Interfaces;
using NLOIQ.Application.Validation;
using NLOIQ.Domain.Content;
namespace NLOIQ.Infrastructure.Data;
public sealed class PublishedContentReader(NloDbContext database) : IPublishedContentReader
{
    private IQueryable<ContentItem> Eligible => database.ContentItems.AsNoTracking().Where(item => item.State == PublicationState.Published && !item.IsDemo);
    public Task<PublishedContentDto?> FindAsync(ContentKind kind, string key, string culture, CancellationToken cancellationToken = default)
    {
        if (!ContentRules.IsCulture(culture)) return Task.FromResult<PublishedContentDto?>(null);
        return (from item in Eligible
                join revision in database.ContentRevisions.AsNoTracking() on new { Owner = item.Id, Revision = item.PublishedRevisionId } equals new { Owner = revision.ContentItemId, Revision = (long?)revision.Id }
                join translation in database.ContentTranslations.AsNoTracking() on revision.Id equals translation.RevisionId
                where item.Kind == kind && item.Key == key && translation.Culture == culture
                select new PublishedContentDto(item.Id, revision.Id, item.Kind, translation.Culture, translation.Title, translation.Summary, translation.Body, translation.Slug))
                .SingleOrDefaultAsync(cancellationToken);
    }
    public Task<PublishedContentDto?> FindRouteAsync(string scope, string slug, string culture, CancellationToken cancellationToken = default)
    {
        if (!ContentRules.IsCulture(culture)) return Task.FromResult<PublishedContentDto?>(null);
        return (from route in database.Set<PublishedRoute>().AsNoTracking()
                join item in Eligible on route.ContentItemId equals item.Id
                join revision in database.ContentRevisions.AsNoTracking() on new { Owner = route.ContentItemId, Revision = route.RevisionId } equals new { Owner = revision.ContentItemId, Revision = revision.Id }
                join translation in database.ContentTranslations.AsNoTracking() on revision.Id equals translation.RevisionId
                where route.Culture == culture && translation.Culture == culture && route.Scope == scope && route.Slug == slug && item.PublishedRevisionId == route.RevisionId
                select new PublishedContentDto(item.Id, revision.Id, item.Kind, translation.Culture, translation.Title, translation.Summary, translation.Body, translation.Slug))
                .SingleOrDefaultAsync(cancellationToken);
    }
}
