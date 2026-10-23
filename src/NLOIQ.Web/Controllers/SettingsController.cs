using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLOIQ.Application.Content;
using NLOIQ.Domain.Content;
namespace NLOIQ.Web.Controllers;
[Authorize(Policy="Administration")]
[Route("{culture:regex(^(en|ar)$)}/admin/settings")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class SettingsController(IContentEditor editor):Controller
{
    [HttpGet("site")]public async Task<IActionResult> Site(CancellationToken ct)=>await Open(ContentKind.SiteSettings,"site",ct);
    [HttpGet("appearance")]public async Task<IActionResult> Appearance(CancellationToken ct)=>await Open(ContentKind.ThemeSettings,"brand",ct);
    private async Task<IActionResult> Open(ContentKind kind,string key,CancellationToken ct){var item=(await editor.ListAsync(kind,null,ct)).SingleOrDefault(x=>x.Key==key);return item is null?NotFound():RedirectToAction("Edit","Cms",new{culture=RouteData.Values["culture"],id=item.Id});}
}
