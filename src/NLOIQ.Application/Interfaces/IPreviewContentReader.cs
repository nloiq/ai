namespace NLOIQ.Application.Interfaces;
public sealed record ServicePreviewDto(string Key, int DisplayOrder, string Title, string Summary, bool IsDemo);
public interface IPreviewContentReader
{
    Task<IReadOnlyList<ServicePreviewDto>> ReadServicesAsync(string culture, CancellationToken cancellationToken);
}
