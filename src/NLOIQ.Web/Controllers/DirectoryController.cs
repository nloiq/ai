using Microsoft.AspNetCore.Mvc;
using NLOIQ.Application.Content;
using NLOIQ.Web.ViewModels;
namespace NLOIQ.Web.Controllers;
public sealed class DirectoryController(IPublicSiteReader reader,Microsoft.Extensions.Localization.IStringLocalizer<NLOIQ.Web.SharedResource> text):Controller
{
    [HttpGet("{culture:regex(^(en|ar)$)}/projects")]
    public Task<IActionResult> Projects(string culture,string? category,CancellationToken ct)=>Open(culture,category,true,ct);
    [HttpGet("{culture:regex(^(en|ar)$)}/services")]
    public Task<IActionResult> Services(string culture,string? category,CancellationToken ct)=>Open(culture,category,false,ct);
    private async Task<IActionResult> Open(string culture,string? category,bool projects,CancellationToken ct)
    {
        var items=projects?await reader.ReadProjectsAsync(culture,ct):await reader.ReadServicesAsync(culture,ct);
        var categories=projects?items.SelectMany(x=>x.CategoryLinks).DistinctBy(x=>x.Key).ToList():items.Select(x=>x.ServiceCategoryKey).Distinct().Select(x=>new PublicCategory(x,text["ServiceCategory_"+x].Value)).ToList();
        if(projects&&items.Any(x=>x.CategoryLinks.Count==0))categories.Add(new("_uncategorized",text["DirectoryGeneral"]));
        categories=categories.OrderBy(x=>x.Label).ToList();
        if(!string.IsNullOrEmpty(category)&&!categories.Any(x=>x.Key==category))return NotFound();
        var intro=projects?await reader.ReadProjectsSectionAsync(culture,ct):await reader.ReadServicesSectionAsync(culture,ct);
        return View("Index",new DirectoryModel(projects,category,items,categories,intro));
    }
    [HttpGet("{culture:regex(^(en|ar)$)}/services/{key}")]
    public async Task<IActionResult> Service(string culture,string key,CancellationToken ct)
    {
        var item=(await reader.ReadServicesAsync(culture,ct)).SingleOrDefault(x=>x.Key==key);return item is null?NotFound():View(item);
    }
}
