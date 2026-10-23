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
    [HttpGet("seo")]public async Task<IActionResult> Seo(CancellationToken ct)=>View((await editor.ListAsync(null,null,ct)).Where(x=>x.Kind is ContentKind.Page or ContentKind.Project).ToList());
    [Authorize(Policy="Administration"),HttpGet("audit")]
    public async Task<IActionResult> Audit(long? contentId,int page=1,CancellationToken ct=default){var query=db.AuditLogs.AsNoTracking();if(contentId is long id)query=query.Where(x=>x.ContentItemId==id);return View(await query.OrderByDescending(x=>x.Id).Skip((Math.Clamp(page,1,10000)-1)*50).Take(50).ToListAsync(ct));}
}
