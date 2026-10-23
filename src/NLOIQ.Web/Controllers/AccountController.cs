using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NLOIQ.Infrastructure.Identity;
using NLOIQ.Web.ViewModels;
namespace NLOIQ.Web.Controllers;
[Route("{culture:regex(^(en|ar)$)}/account")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AccountController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users, Microsoft.Extensions.Localization.IStringLocalizer<NLOIQ.Web.SharedResource> localizer) : Controller
{
    [AllowAnonymous, HttpGet("login")]
    public IActionResult Login(string culture, string? returnUrl = null) =>
        User.Identity?.IsAuthenticated == true ? Redirect($"/{culture}/admin") : View(new LoginModel { ReturnUrl = returnUrl });

    [AllowAnonymous, HttpPost("login"), ValidateAntiForgeryToken, EnableRateLimiting("login")]
    public async Task<IActionResult> Login(string culture, LoginModel model)
    {
        if (ModelState.IsValid) {
            var user = await users.FindByEmailAsync(model.Email.Trim());
            // Hash a dummy password for unknown accounts to avoid an immediate timing shortcut.
            if (user is null) {
                _ = users.PasswordHasher.VerifyHashedPassword(new ApplicationUser(), DummyHash, model.Password);
            }
            else if (user.IsActive) {
                var result = await signIn.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: true);
                if (result.Succeeded) return Url.IsLocalUrl(model.ReturnUrl) ? LocalRedirect(model.ReturnUrl!) : Redirect($"/{culture}/admin");
            }
        }
        {
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, localizer["AdminLoginError"]);
        }
        model.Password = "";
        ModelState.Remove(nameof(model.Password));
        return View(model);
    }
    private static readonly string DummyHash = new PasswordHasher<ApplicationUser>().HashPassword(new ApplicationUser(), Guid.NewGuid().ToString());
    [Authorize, HttpPost("logout"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string culture) { await signIn.SignOutAsync(); return Redirect($"/{culture}/account/login"); }
    [AllowAnonymous, HttpGet("denied")]
    public IActionResult Denied() { Response.StatusCode = 403; return View(); }
}
