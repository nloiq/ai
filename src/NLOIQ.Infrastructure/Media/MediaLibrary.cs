using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLOIQ.Application.Content;
using NLOIQ.Domain.Content;
using NLOIQ.Infrastructure.Data;
namespace NLOIQ.Infrastructure.Media;
public sealed class MediaLibrary(NloDbContext db,IImageOptimizer optimizer,MediaFileStore files,ILogger<MediaLibrary> logger):IMediaLibrary
{
    private static string Version(string en,string ar)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[]{en,ar}))));
    private static void Alt(string en,string ar){if(string.IsNullOrWhiteSpace(en)||string.IsNullOrWhiteSpace(ar)||en.Length>500||ar.Length>500)throw new InvalidOperationException("Both alternative texts are required (maximum 500 characters).");}
    public async Task<IReadOnlyList<MediaRow>> ListAsync(string? search,int page=1,CancellationToken ct=default)
    {
        var query=db.MediaAssets.AsNoTracking();if(!string.IsNullOrWhiteSpace(search))query=query.Where(x=>x.OriginalFileName.Contains(search));
        var rows=await query.OrderByDescending(x=>x.Id).Skip((Math.Clamp(page,1,10000)-1)*30).Take(30).ToListAsync(ct);var result=new List<MediaRow>();
        foreach(var asset in rows){var text=await db.Set<MediaAssetTranslation>().AsNoTracking().Where(x=>x.MediaAssetId==asset.Id).ToListAsync(ct);var en=text.SingleOrDefault(x=>x.Culture=="en")?.AltText??"";var ar=text.SingleOrDefault(x=>x.Culture=="ar")?.AltText??"";result.Add(new(asset.Id,asset.OriginalFileName,asset.Width,asset.Height,asset.ByteSize,asset.IsDemo,await db.Set<RevisionMedia>().AnyAsync(x=>x.MediaAssetId==asset.Id,ct),en,ar,Version(en,ar)));}return result;
    }
    public async Task<long> UploadAsync(Stream stream,string filename,string contentType,string altEn,string altAr,Guid actor,string actorLabel,CancellationToken ct=default)
    {
        Alt(altEn,altAr);var image=await optimizer.OptimizeAsync(stream,filename,contentType,ct);
        var key=Guid.NewGuid().ToString("N")+"/main.webp";var commitStarted=false;
        try {
            await files.WriteAsync(key,image.Files,ct);await using var tx=await db.Database.BeginTransactionAsync(ct);
            var name=Path.GetFileName(filename.Replace('\\','/'));name=new string(name.Where(x=>!char.IsControl(x)).ToArray());if(name.Length>255)name=name[..255];
            var asset=new MediaAsset{StorageProvider="Local",StorageKey=key,OriginalFileName=name,ContentType="image/webp",ByteSize=image.Files["main.webp"].LongLength,Width=image.Width,Height=image.Height,Sha256=Convert.ToHexString(SHA256.HashData(image.Files["main.webp"])).ToLowerInvariant()};db.Add(asset);
            db.Add(new MediaAssetTranslation{MediaAsset=asset,Culture="en",AltText=altEn});db.Add(new MediaAssetTranslation{MediaAsset=asset,Culture="ar",AltText=altAr});await db.SaveChangesAsync(ct);
            db.AuditLogs.Add(new(){ActorUserId=actor,ActorLabel=actorLabel[..Math.Min(actorLabel.Length,160)],Action="UploadMedia",DetailsJson=JsonSerializer.Serialize(new{asset.Id,image.Width,image.Height,asset.ByteSize})});await db.SaveChangesAsync(ct);commitStarted=true;await tx.CommitAsync(ct);return asset.Id;
        }catch{if(!commitStarted)files.Delete(key);throw;}
    }
    private async Task<MediaAsset> Locked(long id,CancellationToken ct)=>(await db.MediaAssets.FromSqlInterpolated($"SELECT * FROM MediaAssets WHERE Id={id} FOR UPDATE").ToListAsync(ct)).SingleOrDefault()??throw new KeyNotFoundException();
    public async Task UpdateAltAsync(long id,string version,string altEn,string altAr,Guid actor,string actorLabel,CancellationToken ct=default)
    {
        Alt(altEn,altAr);await using var tx=await db.Database.BeginTransactionAsync(ct);_ = await Locked(id,ct);
        var texts=await db.Set<MediaAssetTranslation>().Where(x=>x.MediaAssetId==id).ToListAsync(ct);var en=texts.SingleOrDefault(x=>x.Culture=="en");var ar=texts.SingleOrDefault(x=>x.Culture=="ar");
        if(version!=Version(en?.AltText??"",ar?.AltText??""))throw new ContentConflictException();
        foreach(var (culture,value) in new[]{("en",altEn),("ar",altAr)}){var text=texts.SingleOrDefault(x=>x.Culture==culture);if(text is null)db.Add(new MediaAssetTranslation{MediaAssetId=id,Culture=culture,AltText=value});else text.AltText=value;}
        db.AuditLogs.Add(new(){ActorUserId=actor,ActorLabel=actorLabel[..Math.Min(actorLabel.Length,160)],Action="EditMediaAltHints",DetailsJson=JsonSerializer.Serialize(new{mediaId=id})});await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
    public async Task DeleteAsync(long id,Guid actor,string actorLabel,CancellationToken ct=default)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);var asset=await Locked(id,ct);
        if(await db.Set<RevisionMedia>().AnyAsync(x=>x.MediaAssetId==id,ct))throw new InvalidOperationException("Media is referenced by an immutable revision and cannot be deleted.");
        db.RemoveRange(await db.Set<MediaAssetTranslation>().Where(x=>x.MediaAssetId==id).ToListAsync(ct));db.Remove(asset);
        db.AuditLogs.Add(new(){ActorUserId=actor,ActorLabel=actorLabel[..Math.Min(actorLabel.Length,160)],Action="DeleteUnusedMedia",DetailsJson=JsonSerializer.Serialize(new{mediaId=id})});await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        if(asset.StorageProvider=="Local")try{files.Delete(asset.StorageKey);}catch(Exception error)when(error is IOException or UnauthorizedAccessException){logger.LogWarning("Deleted media {Id} has orphaned local files requiring cleanup.",id);}
    }
}
