using NLOIQ.Domain.Content;
namespace NLOIQ.Application.Interfaces;
public sealed record PublishedContentDto(long ContentItemId, long RevisionId, ContentKind Kind, string Culture, string Title, string Summary, string Body, string? Slug);
public interface IPublishedContentReader
{
    Task<PublishedContentDto?> FindAsync(ContentKind kind, string key, string culture, CancellationToken cancellationToken = default);
    Task<PublishedContentDto?> FindRouteAsync(string scope, string slug, string culture, CancellationToken cancellationToken = default);
}
