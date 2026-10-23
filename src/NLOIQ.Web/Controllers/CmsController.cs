using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NLOIQ.Application.Content;
using NLOIQ.Domain.Content;
namespace NLOIQ.Web.Controllers;
[Authorize(Policy="ContentEdit"),AutoValidateAntiforgeryToken]
[Route("{culture:regex(^(en|ar)$)}/admin/content")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
[RequestSizeLimit(256000)]
public sealed class CmsController(IContentEditor editor,NLOIQ.Application.Interfaces.IPublishedContentReader published,IStringLocalizer<SharedResource> localizer,ILogger<CmsController> logger):Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(ContentKind? kind,SectionKind? sectionKind,string? search,CancellationToken ct) {
        var rows=await editor.ListAsync(kind,search,ct);if(!User.IsInRole("SuperAdmin"))rows=rows.Where(x=>x.Kind is not (ContentKind.SiteSettings or ContentKind.ThemeSettings)).ToList();if(sectionKind.HasValue)rows=rows.Where(x=>x.Kind==ContentKind.Section&&x.SectionKind==sectionKind).ToList();return View(rows);
    }
    [HttpGet("new")]
    public async Task<IActionResult> Create(ContentKind kind=ContentKind.Page,SectionKind sectionKind=SectionKind.Hero,CancellationToken ct=default)
    {
        if(kind is ContentKind.SiteSettings or ContentKind.ThemeSettings || !Enum.IsDefined(kind))return NotFound();
        var key=kind switch{ContentKind.Navigation or ContentKind.Footer=>"main",ContentKind.Contact=>"primary",_=>""};
        return await EditView(new(){Kind=kind,SectionKind=sectionKind,Key=key},ct);
    }
    [HttpGet("{id:long}")]
    public async Task<IActionResult> Edit(long id,CancellationToken ct)
    {
        var model=await editor.GetAsync(id,ct);return model is null?NotFound():model.Kind is ContentKind.SiteSettings or ContentKind.ThemeSettings && !User.IsInRole("SuperAdmin")?Forbid():await EditView(model,ct);
    }
    private async Task<IActionResult> EditView(ContentEdit model,CancellationToken ct)
    {
        ViewData["Choices"]=await editor.ChoicesAsync(ct);
        if(model.Id!=0)ViewData["Item"]=(await editor.ListAsync(model.Kind,null,ct)).Single(x=>x.Id==model.Id);
        if(model.Id!=0 && model.Kind is ContentKind.Page or ContentKind.Project) {
            var culture=RouteData.Values["culture"]?.ToString()??"en";
            var live=await published.FindAsync(model.Kind,model.Key,culture,ct);
            if(live is not null)ViewData["PublishedPath"]=model.Kind==ContentKind.Page&&model.Key=="home"?$"/{culture}/":$"/{culture}/{(model.Kind==ContentKind.Page?"pages":"projects")}/{live.Slug}";
        }
        return View("Edit",model);
    }
    [HttpPost("new"),HttpPost("{id:long}")]
    public async Task<IActionResult> Save(long? id,ContentEdit model,CancellationToken ct)
    {
        if((id??0)!=model.Id)return BadRequest();
        if(model.Id!=0){var existing=await editor.GetAsync(model.Id,ct);if(existing is null)return NotFound();if(existing.Kind is ContentKind.SiteSettings or ContentKind.ThemeSettings && !User.IsInRole("SuperAdmin"))return Forbid();if(existing.Kind!=model.Kind||existing.Key!=model.Key)return BadRequest();}
        if(model.Kind is ContentKind.SiteSettings or ContentKind.ThemeSettings && !User.IsInRole("SuperAdmin"))return Forbid();
        if(ModelState.IsValid)try {
            var saved=await editor.SaveAsync(model,Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),User.Identity!.Name!,ct);
            TempData["CmsNotice"]=localizer["CmsSaved"].Value;return RedirectToAction(nameof(Edit),new{id=saved,culture=RouteData.Values["culture"]});
        }catch(ContentConflictException){Response.StatusCode=409;ModelState.AddModelError("",localizer["CmsConflict"]);}
        catch(Exception error)when(error is InvalidOperationException or ValidationException or DbUpdateException) {
            logger.LogWarning("CMS draft rejected for item {Id}: {ErrorType}",model.Id,error.GetType().Name);
            ModelState.AddModelError("",localizer["CmsValidation"]+(RouteData.Values["culture"]?.ToString()=="en"&&error is not DbUpdateException?" "+error.Message:""));
        }
        return await EditView(model,ct);
    }
    [Authorize(Policy="ContentPublish"),HttpPost("{id:long}/status")]
    public async Task<IActionResult> Status(long id,long version,string action,bool verified,CancellationToken ct)
    {
        try {
            await editor.TransitionAsync(id,version,action,verified,Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),User.Identity!.Name!,ct);
            TempData["CmsNotice"]=localizer["CmsStatusUpdated"].Value;
        }catch(KeyNotFoundException){return NotFound();}
        catch(ContentConflictException){TempData["CmsError"]=localizer["CmsConflict"].Value;}
        catch(Exception error)when(error is InvalidOperationException or DbUpdateException) {
            logger.LogWarning("CMS publication rejected for item {Id}: {ErrorType}",id,error.GetType().Name);
            TempData["CmsError"]=localizer["CmsPublishError"].Value+(RouteData.Values["culture"]?.ToString()=="en"&&error is not DbUpdateException?" "+error.Message:"");
        }
        return RedirectToAction(nameof(Edit),new{id,culture=RouteData.Values["culture"]});
    }
}
