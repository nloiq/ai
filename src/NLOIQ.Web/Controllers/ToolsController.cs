using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLOIQ.Application.Content;
using NLOIQ.Domain.Content;
using NLOIQ.Infrastructure.Data;
namespace NLOIQ.Web.Controllers;
[Authorize(Policy="ContentEdit")]
[Route("{culture:regex(^(en|ar)$)}/admin")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class ToolsController(IContentEditor editor,NloDbContext db):Controller
{
    [HttpGet("translations")]
    public async Task<IActionResult> Translations(string? search, int page=1, CancellationToken ct=default)
    {
        page=Math.Clamp(page,1,10000);
        var query=db.ContentItems.AsNoTracking().AsQueryable();
        if(!User.IsInRole("SuperAdmin"))query=query.Where(x=>x.Kind!=ContentKind.SiteSettings&&x.Kind!=ContentKind.ThemeSettings);
        if(!string.IsNullOrWhiteSpace(search))query=query.Where(x=>x.Key.Contains(search));
        var roots=await query.OrderBy(x=>x.Kind).ThenBy(x=>x.Key).Skip((page-1)*50).Take(51).ToListAsync(ct);
        var hasNext=roots.Count>50;roots=roots.Take(50).ToList();
        var revisions=roots.Where(x=>x.DraftRevisionId.HasValue).Select(x=>x.DraftRevisionId!.Value).ToArray();
        var text=await db.ContentTranslations.AsNoTracking().Where(x=>revisions.Contains(x.RevisionId)).ToListAsync(ct);
        var media=await db.Set<RevisionMedia>().AsNoTracking().Where(x=>revisions.Contains(x.RevisionId)).ToListAsync(ct);
        var alt=await db.Set<RevisionMediaTranslation>().AsNoTracking().Where(x=>revisions.Contains(x.RevisionId)).ToListAsync(ct);
        var rows=roots.Select(root=>{
            var en=text.SingleOrDefault(x=>x.RevisionId==root.DraftRevisionId&&x.Culture=="en");
            var ar=text.SingleOrDefault(x=>x.RevisionId==root.DraftRevisionId&&x.Culture=="ar");
            List<string> Missing(string culture,ContentTranslation? current,ContentTranslation? other) {
                var fields=new List<string>();
                void Need(string key,string? value,bool required){if(required&&string.IsNullOrWhiteSpace(value))fields.Add(key);}
                Need("Title",current?.Title,true);Need("Slug",current?.Slug,root.Kind is ContentKind.Page or ContentKind.Project);
                Need("Summary",current?.Summary,!string.IsNullOrWhiteSpace(other?.Summary));
                Need("Body",current?.Body,!string.IsNullOrWhiteSpace(other?.Body));
                Need("Primary",current?.PrimaryButtonLabel,!string.IsNullOrWhiteSpace(other?.PrimaryButtonLabel));
                Need("Secondary",current?.SecondaryButtonLabel,!string.IsNullOrWhiteSpace(other?.SecondaryButtonLabel));
                foreach(var image in media.Where(x=>x.RevisionId==root.DraftRevisionId)) {
                    var own=alt.SingleOrDefault(x=>x.RevisionMediaId==image.Id&&x.Culture==culture);
                    Need("Alt",own?.AltText,true);
                    Need("Caption",own?.Caption,alt.Any(x=>x.RevisionMediaId==image.Id&&x.Culture!=culture&&!string.IsNullOrWhiteSpace(x.Caption)));
                }
                return fields.Distinct().ToList();
            }
            return new NLOIQ.Web.Models.TranslationReview(root.Id,root.Kind,root.Key,root.State,root.IsDemo,en?.Title??"",ar?.Title??"",Missing("en",en,ar),Missing("ar",ar,en));
        }).ToList();
        return View(new NLOIQ.Web.Models.TranslationReviewPage(rows,page,hasNext));
    }
    [HttpGet("seo")]public async Task<IActionResult> Seo(CancellationToken ct)=>View((await editor.ListAsync(null,null,ct,RouteData.Values["culture"]?.ToString()??"en")).Where(x=>x.Kind is ContentKind.Page or ContentKind.Project).ToList());
    [Authorize(Policy="Administration"),HttpGet("audit")]
    public async Task<IActionResult> Audit(long? contentId,int page=1,CancellationToken ct=default){var query=db.AuditLogs.AsNoTracking();if(contentId is long id)query=query.Where(x=>x.ContentItemId==id);return View(await query.OrderByDescending(x=>x.Id).Skip((Math.Clamp(page,1,10000)-1)*50).Take(50).ToListAsync(ct));}
}
