using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NLOIQ.Application.Content;
using NLOIQ.Domain.Content;
using NLOIQ.Infrastructure.Data;
using NLOIQ.Infrastructure.Media;
namespace NLOIQ.Web.Controllers;
[Authorize(Policy="ContentEdit"),AutoValidateAntiforgeryToken]
[Route("{culture:regex(^(en|ar)$)}/admin/content/{id:long}")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class WorkflowController(IContentWorkflow workflow,IContentPreviewReader previews,IContentEditor editor,NloDbContext db,MediaFileStore files,IStringLocalizer<SharedResource> text):Controller
{
    private string Culture=>RouteData.Values["culture"]?.ToString()=="ar"?"ar":"en";
    private Guid Actor=>Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private async Task<IActionResult?> Guard(long id,CancellationToken ct){var kind=await db.ContentItems.AsNoTracking().Where(x=>x.Id==id).Select(x=>(ContentKind?)x.Kind).SingleOrDefaultAsync(ct);return kind is null?NotFound():kind is ContentKind.SiteSettings or ContentKind.ThemeSettings&&!User.IsInRole("SuperAdmin")?Forbid():null;}
    [HttpGet("history")]
    public async Task<IActionResult> History(long id,int page=1,CancellationToken ct=default){if(await Guard(id,ct) is { } denied)return denied;return View(await workflow.HistoryAsync(id,page,ct));}
    [HttpGet("preview")]
    public async Task<IActionResult> Latest(long id,CancellationToken ct){if(await Guard(id,ct) is { } denied)return denied;var revision=await db.ContentItems.Where(x=>x.Id==id).Select(x=>x.DraftRevisionId).SingleAsync(ct);return revision is null?NotFound():RedirectToAction(nameof(Preview),new{id,revisionId=revision.Value,culture=Culture});}
    [HttpGet("revisions/{revisionId:long}/preview")]
    public async Task<IActionResult> Preview(long id,long revisionId,CancellationToken ct)
    {
        if(await Guard(id,ct) is { } denied)return denied;
        Response.Headers["X-Robots-Tag"]="noindex, nofollow, noarchive";
        var model=await previews.PreviewAsync(id,revisionId,Culture,ct);if(model is null)return NotFound();
        if(model.Page.Kind is ContentKind.SiteSettings or ContentKind.ThemeSettings){ViewData["PreviewSite"]=model;ViewData["Choices"]=await editor.ChoicesAsync(ct,Culture);return View("SettingsPreview",await editor.GetRevisionAsync(id,revisionId,ct));}
        return View("~/Views/Home/Cms.cshtml",model);
    }
    [HttpGet("revisions/{revisionId:long}/media/{assetId:long}/{size}")]
    public async Task<IActionResult> Image(long id,long revisionId,long assetId,string size,CancellationToken ct)
    {
        if(await Guard(id,ct) is { } denied)return denied;
        if(!await db.Set<RevisionMedia>().AnyAsync(x=>x.RevisionId==revisionId&&x.MediaAssetId==assetId&&x.Revision.ContentItemId==id,ct))return NotFound();
        var asset=await db.MediaAssets.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==assetId&&x.StorageProvider=="Local",ct);if(asset is null)return NotFound();
        var path=files.Find(asset.StorageKey,size);return path is null?NotFound():PhysicalFile(path,size=="icon"?"image/png":"image/webp");
    }
    [HttpPost("restore")]
    public async Task<IActionResult> Restore(long id,long revisionId,long version,bool confirm,CancellationToken ct)
    {
        if(await Guard(id,ct) is { } denied)return denied;
        return await Change(id,async()=>{if(!confirm)throw new InvalidOperationException("Confirm restoration before continuing.");await workflow.RestoreAsync(id,revisionId,version,Actor,User.Identity!.Name!,ct);},"WorkflowRestored");
    }
    [Authorize(Policy="ContentPublish"),HttpPost("schedule")]
    public async Task<IActionResult> Schedule(long id,long revisionId,long version,string? scheduledLocal,bool verified,CancellationToken ct)
    {
        if(await Guard(id,ct) is { } denied)return denied;
        return await Change(id,async()=>{
            if(!DateTime.TryParseExact(scheduledLocal,"yyyy-MM-ddTHH:mm",CultureInfo.InvariantCulture,DateTimeStyles.None,out var local))throw new InvalidOperationException("Choose a valid Baghdad publication time.");
            var zone=TimeZoneInfo.FindSystemTimeZoneById("Asia/Baghdad");local=DateTime.SpecifyKind(local,DateTimeKind.Unspecified);
            if(zone.IsInvalidTime(local)||zone.IsAmbiguousTime(local))throw new InvalidOperationException("Choose a valid Baghdad publication time.");
            await workflow.ScheduleAsync(id,revisionId,version,TimeZoneInfo.ConvertTimeToUtc(local,zone),verified,Actor,User.Identity!.Name!,ct);
        },"WorkflowScheduled");
    }
    [Authorize(Policy="ContentPublish"),HttpPost("schedules/{scheduleId:long}/cancel")]
    public async Task<IActionResult> Cancel(long id,long scheduleId,long version,long scheduleVersion,CancellationToken ct)
    {
        if(await Guard(id,ct) is { } denied)return denied;
        return await Change(id,()=>workflow.CancelAsync(id,scheduleId,version,scheduleVersion,Actor,User.Identity!.Name!,ct),"WorkflowCancelled");
    }
    private async Task<IActionResult> Change(long id,Func<Task> action,string notice)
    {
        try{await action();TempData["CmsNotice"]=text[notice].Value;}
        catch(KeyNotFoundException){return NotFound();}
        catch(ContentConflictException){TempData["CmsError"]=text["CmsConflict"].Value;}
        catch(Exception error)when(error is InvalidOperationException or DbUpdateException){var detail=error is InvalidOperationException?text[error.Message]:null;TempData["CmsError"]=text["WorkflowError"].Value+(detail is {ResourceNotFound:false}?" "+detail.Value:"");}
        return RedirectToAction(nameof(History),new{id,culture=Culture});
    }
}
