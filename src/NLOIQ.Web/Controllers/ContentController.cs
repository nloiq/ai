using Microsoft.AspNetCore.Mvc;
using NLOIQ.Application.Content;
namespace NLOIQ.Web.Controllers;
public sealed class ContentController(IPublicSiteReader reader):Controller
{
    [HttpGet("{culture:regex(^(en|ar)$)}/pages/{slug}")]
    public async Task<IActionResult> Page(string culture,string slug,CancellationToken ct){var site=await reader.ReadAsync(culture,slug,ct:ct);return site is null?NotFound():View("~/Views/Home/Cms.cshtml",site);}
    [HttpGet("{culture:regex(^(en|ar)$)}/projects/{slug}")]
    public async Task<IActionResult> Project(string culture,string slug,CancellationToken ct){var site=await reader.ReadAsync(culture,slug,true,ct);return site is null?NotFound():View("~/Views/Home/Cms.cshtml",site);}
}
