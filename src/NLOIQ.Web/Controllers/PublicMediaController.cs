using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLOIQ.Infrastructure.Data;
using NLOIQ.Infrastructure.Media;
using NLOIQ.Application.Content;
namespace NLOIQ.Web.Controllers;
public sealed class PublicMediaController(NloDbContext db,MediaFileStore files,IBrandReader brand):Controller
{
    [HttpGet("/media/{id:long}/{size}")]
    public async Task<IActionResult> Image(long id,string size,CancellationToken ct)
    {
        var asset=await db.MediaAssets.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id&&x.StorageProvider=="Local",ct);
        if(asset is null||!await MediaAccess.IsPublicAsync(db,id,ct))return NotFound();
        var path=files.Find(asset.StorageKey,size);if(path is null)return NotFound();
        Response.Headers.CacheControl="public, max-age=3600";Response.Headers.ETag="\""+asset.Sha256+"-"+size+"\"";
        return PhysicalFile(path,size=="icon"?"image/png":"image/webp");
    }
    [HttpGet("/brand/{revision:long}.css")]
    public async Task<IActionResult> Theme(long revision,CancellationToken ct){var css=await brand.ThemeCssAsync(revision,ct);if(css is null)return NotFound();Response.Headers.CacheControl="public, max-age=3600";return Content(css,"text/css");}
}
