using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLOIQ.Infrastructure.Data;
namespace NLOIQ.Web.Controllers;
[Authorize(Policy="ContentEdit"),AutoValidateAntiforgeryToken]
[Route("{culture:regex(^(en|ar)$)}/admin/inquiries")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class InquiriesController(NloDbContext db):Controller
{
    [HttpGet("/{culture:regex(^(en|ar)$)}/inquiries")]
    public IActionResult Shortcut(string culture) => LocalRedirect($"/{culture}/admin/inquiries");
    [HttpGet("")]
    public async Task<IActionResult> Index(int page=1,CancellationToken ct=default){page=Math.Clamp(page,1,10000);ViewData["Page"]=page;ViewData["HasNext"]=await db.ContactInquiries.CountAsync(ct)>page*25;return View(await db.ContactInquiries.AsNoTracking().OrderBy(x=>x.IsRead).ThenByDescending(x=>x.CreatedUtc).ThenByDescending(x=>x.Id).Skip((page-1)*25).Take(25).ToListAsync(ct));}
    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(long id,CancellationToken ct){var item=await db.ContactInquiries.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);return item is null?NotFound():View(item);}
    [HttpPost("{id:long}/read")]
    public async Task<IActionResult> Read(string culture,long id,CancellationToken ct){var item=await db.ContactInquiries.SingleOrDefaultAsync(x=>x.Id==id,ct);if(item is null)return NotFound();item.IsRead=true;await db.SaveChangesAsync(ct);return RedirectToAction(nameof(Details),new{culture,id});}
}
