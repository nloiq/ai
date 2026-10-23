using Microsoft.AspNetCore.Mvc;
using NLOIQ.Application.Interfaces;
namespace NLOIQ.Web.Controllers;
public sealed class HomeController(IPreviewContentReader reader, IWebHostEnvironment environment, Microsoft.Extensions.Options.IOptions<NLOIQ.Web.Configuration.PreviewOptions> previewOptions) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string culture, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment()) return NotFound();
        if (culture is not ("en" or "ar")) return NotFound();
        var services = await reader.ReadServicesAsync(culture, cancellationToken);
        return View(new NLOIQ.Web.ViewModels.HomeViewModel(culture, services, previewOptions.Value.VideoIds));
    }
}
