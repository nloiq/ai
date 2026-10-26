using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLOIQ.Application.Interfaces;
namespace NLOIQ.Web.Controllers;
public sealed class HomeController(IPreviewContentReader reader, NLOIQ.Infrastructure.Data.NloDbContext database, NLOIQ.Application.Content.IPublicSiteReader siteReader, IWebHostEnvironment environment, Microsoft.Extensions.Options.IOptions<NLOIQ.Web.Configuration.PreviewOptions> previewOptions) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string culture, CancellationToken cancellationToken)
    {
        var site = await siteReader.ReadAsync(culture, ct: cancellationToken);
        if (site is not null) return View("Cms", site);
        if (!environment.IsDevelopment() || await database.ContentItems.AnyAsync(x => x.Kind == NLOIQ.Domain.Content.ContentKind.Page && x.Key == "home" && !x.IsDemo, cancellationToken)) return NotFound();
        if (culture is not ("en" or "ar")) return NotFound();
        var services = await reader.ReadServicesAsync(culture, cancellationToken);
        return View(new NLOIQ.Web.ViewModels.HomeViewModel(culture, services, previewOptions.Value.VideoIds) { Contact=await siteReader.ReadContactAsync(culture,cancellationToken), Hero=await siteReader.ReadHeroAsync(culture,cancellationToken), ProjectsSection=await siteReader.ReadProjectsSectionAsync(culture,cancellationToken) });
    }
}
