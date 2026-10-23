using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NLOIQ.Application.Content;
using NLOIQ.Infrastructure.Data;
using NLOIQ.Infrastructure.Media;
namespace NLOIQ.Web.Controllers;
[Authorize(Policy="ContentEdit"),AutoValidateAntiforgeryToken]
[Route("{culture:regex(^(en|ar)$)}/admin/media")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class MediaController(IMediaLibrary library,NloDbContext db,MediaFileStore files,IWebHostEnvironment environment,IStringLocalizer<SharedResource> localizer):Controller
{
    [HttpGet("")]public async Task<IActionResult> Index(string? search,int page=1,CancellationToken ct=default)=>View(await library.ListAsync(search,page,ct));
    private Guid Actor=>Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpPost("upload"),RequestSizeLimit(9*1024*1024),RequestFormLimits(MultipartBodyLengthLimit=9*1024*1024),EnableRateLimiting("upload")]
    public async Task<IActionResult> Upload(IFormFile? image,string? altEn,string? altAr,CancellationToken ct)
    {
        try {
            if(image is null||image.Length<=0||image.Length>ImageOptimizer.MaxBytes)throw new InvalidOperationException("Choose an image up to 8 MiB.");
            await using var stream=image.OpenReadStream();await library.UploadAsync(stream,image.FileName,image.ContentType,altEn??"",altAr??"",Actor,User.Identity!.Name!,ct);TempData["MediaNotice"]=localizer["MediaUploaded"].Value;
        }catch(Exception error)when(error is InvalidOperationException or ArgumentException or DbUpdateException){TempData["MediaError"]=localizer["MediaInvalid"].Value;}
        return RedirectToAction(nameof(Index),new{culture=RouteData.Values["culture"]});
    }
    [HttpPost("{id:long}/alt")]
    public async Task<IActionResult> Alt(long id,string version,string? altEn,string? altAr,CancellationToken ct)
    {
        try{await library.UpdateAltAsync(id,version,altEn??"",altAr??"",Actor,User.Identity!.Name!,ct);TempData["MediaNotice"]=localizer["MediaAltSaved"].Value;}
        catch(KeyNotFoundException){return NotFound();}catch(ContentConflictException){TempData["MediaError"]=localizer["CmsConflict"].Value;}catch(InvalidOperationException){TempData["MediaError"]=localizer["MediaInvalid"].Value;}
        return RedirectToAction(nameof(Index),new{culture=RouteData.Values["culture"]});
    }
    [Authorize(Policy="Administration"),HttpPost("{id:long}/delete")]
    public async Task<IActionResult> Delete(long id,CancellationToken ct)
    {
        try{await library.DeleteAsync(id,Actor,User.Identity!.Name!,ct);TempData["MediaNotice"]=localizer["MediaDeleted"].Value;}
        catch(KeyNotFoundException){return NotFound();}catch(Exception error)when(error is InvalidOperationException or DbUpdateException){TempData["MediaError"]=localizer["MediaInUse"].Value;}
        return RedirectToAction(nameof(Index),new{culture=RouteData.Values["culture"]});
    }
    [HttpGet("{id:long}/preview")]
    public async Task<IActionResult> Preview(long id,CancellationToken ct)
    {
        var asset=await db.MediaAssets.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);if(asset is null)return NotFound();
        if(asset.StorageProvider=="Local"){var local=files.Find(asset.StorageKey,"thumb");return local is null?NotFound():PhysicalFile(local,"image/webp");}
        if(asset.StorageProvider!="Bundled"||!asset.StorageKey.StartsWith("images/",StringComparison.Ordinal)||asset.StorageKey.Contains("..",StringComparison.Ordinal)||asset.StorageKey.Contains('\\'))return NotFound();
        var path=Path.GetFullPath(Path.Combine(environment.WebRootPath,asset.StorageKey));var root=Path.GetFullPath(environment.WebRootPath)+Path.DirectorySeparatorChar;
        return path.StartsWith(root,StringComparison.OrdinalIgnoreCase)&&System.IO.File.Exists(path)?PhysicalFile(path,asset.ContentType):NotFound();
    }
}
