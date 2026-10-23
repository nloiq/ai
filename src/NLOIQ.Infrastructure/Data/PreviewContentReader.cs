using Microsoft.EntityFrameworkCore;
using NLOIQ.Application.Interfaces;
namespace NLOIQ.Infrastructure.Data;
public sealed class PreviewContentReader(NloDbContext database) : IPreviewContentReader
{
    public async Task<IReadOnlyList<ServicePreviewDto>> ReadServicesAsync(string culture, CancellationToken cancellationToken)
        => await database.ServicePreviews.AsNoTracking().Where(x => x.IsDemo).OrderBy(x => x.DisplayOrder)
            .SelectMany(x => x.Translations.Where(t => t.Culture == culture)
                .Select(t => new ServicePreviewDto(x.Key, x.DisplayOrder, t.Title, t.Summary, x.IsDemo)))
            .ToListAsync(cancellationToken);
}
