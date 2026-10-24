using Microsoft.EntityFrameworkCore;
using NLOIQ.Domain.Content;
using NLOIQ.Infrastructure.Data;
using NLOIQ.Application.Content;
namespace NLOIQ.Infrastructure.Media;
public static class MediaAccess
{
    public static Task<bool> IsPublicAsync(NloDbContext db,long id,CancellationToken ct=default)=>db.Set<RevisionMedia>().AsNoTracking().AnyAsync(media=>media.MediaAssetId==id&&!media.MediaAsset.IsDemo&&media.Revision.ContentItem.State==PublicationState.Published&&!media.Revision.ContentItem.IsDemo&&
        (media.Revision.ContentItem.PublishedRevisionId==media.RevisionId||db.Set<PageSection>().Any(link=>link.SectionRevisionId==media.RevisionId&&link.Revision.ContentItem.PublishedRevisionId==link.RevisionId&&link.Revision.ContentItem.State==PublicationState.Published&&!link.Revision.ContentItem.IsDemo)),ct);
    public static PublicImage? Image(MediaAsset asset,string alt,string? caption=null,bool preview=false){
        if(asset.IsDemo&&!preview)return null;
        if(asset.StorageProvider=="Local") {var width=asset.Width??1;var url=$"/media/{asset.Id}/full";var sizes=new List<string>();foreach(var size in new[]{640,1280})if(width>size)sizes.Add($"/media/{asset.Id}/{size} {size}w");sizes.Add($"{url} {width}w");return new(url,alt,caption,width,asset.Height??1,string.Join(", ",sizes));}
        if(asset.StorageProvider=="Bundled"&&asset.StorageKey.StartsWith("images/",StringComparison.Ordinal)&&!asset.StorageKey.Contains("..",StringComparison.Ordinal)&&!asset.StorageKey.Contains('\\'))return new("/"+asset.StorageKey,alt,caption,asset.Width??840,asset.Height??600);
        return null;
    }
}
