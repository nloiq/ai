namespace NLOIQ.Application.Content;
public sealed record OptimizedImage(int Width,int Height,IReadOnlyDictionary<string,byte[]> Files);
public interface IImageOptimizer { Task<OptimizedImage> OptimizeAsync(Stream source,string filename,string contentType,CancellationToken ct=default); }
public sealed record MediaRow(long Id,string Name,int? Width,int? Height,long Bytes,bool IsDemo,bool InUse,string AltEn,string AltAr,string Version);
public interface IMediaLibrary
{
    Task<IReadOnlyList<MediaRow>> ListAsync(string? search,int page=1,CancellationToken ct=default);
    Task<long> UploadAsync(Stream stream,string filename,string contentType,string altEn,string altAr,Guid actor,string actorLabel,CancellationToken ct=default);
    Task UpdateAltAsync(long id,string version,string altEn,string altAr,Guid actor,string actorLabel,CancellationToken ct=default);
    Task DeleteAsync(long id,Guid actor,string actorLabel,CancellationToken ct=default);
}
public sealed record PublicSeo(string Title,string Description,string? OpenGraphTitle,string? OpenGraphDescription,PublicImage? Image,string? CanonicalPath,bool NoIndex);
public sealed record PublicBrand(string SiteName,string DefaultCulture,string DefaultTheme,string? BaseUrl,PublicImage? Logo,string? FaviconUrl,long? ThemeRevision,IReadOnlyList<PublicLink> Social);
public interface IBrandReader { Task<PublicBrand> ReadAsync(string culture,CancellationToken ct=default); Task<string?> ThemeCssAsync(long revision,CancellationToken ct=default); }
