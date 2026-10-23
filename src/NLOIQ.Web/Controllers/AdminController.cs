using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLOIQ.Infrastructure.Data;
using NLOIQ.Domain.Content;
using NLOIQ.Web.ViewModels;
namespace NLOIQ.Web.Controllers;
[Authorize(Policy = "AdminAccess")]
[Route("{culture:regex(^(en|ar)$)}/admin")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AdminController(NloDbContext database) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(new AdminDashboardModel(
        await database.ContentItems.CountAsync(cancellationToken),
        await database.ContentItems.CountAsync(x => x.State == PublicationState.Draft, cancellationToken),
        await database.ContentItems.CountAsync(x => x.State == PublicationState.Published, cancellationToken),
        await database.MediaAssets.CountAsync(cancellationToken),
        User.IsInRole("SuperAdmin") ? await database.Users.CountAsync(cancellationToken) : 0));
    [Authorize(Policy = "ContentEdit"), HttpGet("editor")]
    public IActionResult Editor(string culture) => Redirect($"/{culture}/admin/content");
    [Authorize(Policy = "Administration"), HttpGet("settings")]
    public IActionResult Settings(string culture) => Redirect($"/{culture}/admin/settings/site");
}
