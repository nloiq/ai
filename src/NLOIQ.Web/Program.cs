using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using NLOIQ.Infrastructure.Identity;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using NLOIQ.Infrastructure;
using NLOIQ.Infrastructure.Data;
var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
        .AddEnvironmentVariables().AddCommandLine(args);
}
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection using local configuration or environment variables.");
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddOptions<NLOIQ.Web.Configuration.PreviewOptions>()
    .Bind(builder.Configuration.GetSection("Preview"))
    .Validate(options => options.IsValid(), "Preview video settings require known project keys and valid 11-character YouTube IDs.")
    .ValidateOnStart();
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options => {
    options.Password.RequiredLength = 14;
    options.Password.RequiredUniqueChars = 4;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
}).AddEntityFrameworkStores<NloDbContext>().AddDefaultTokenProviders();
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.Name = "NLOIQ.Admin";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context => {
        var culture = context.Request.RouteValues["culture"]?.ToString() == "ar" ? "ar" : "en";
        context.Response.Redirect($"/{culture}/account/login?returnUrl={Uri.EscapeDataString(context.Request.PathBase + context.Request.Path + context.Request.QueryString)}");
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context => {
        var culture = context.Request.RouteValues["culture"]?.ToString() == "ar" ? "ar" : "en";
        context.Response.Redirect($"/{culture}/account/denied");
        return Task.CompletedTask;
    };
    options.Events.OnValidatePrincipal = async context => {
        await SecurityStampValidator.ValidatePrincipalAsync(context);
        var manager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = context.Principal is null ? null : await manager.GetUserAsync(context.Principal);
        if (user is null || !user.IsActive || await manager.IsLockedOutAsync(user)) {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        }
    };
});
builder.Services.AddAuthorization(options => {
    options.AddPolicy("AdminAccess", policy => policy.RequireAuthenticatedUser().RequireRole("SuperAdmin", "Editor", "Viewer"));
    options.AddPolicy("ContentEdit", policy => policy.RequireAuthenticatedUser().RequireRole("SuperAdmin", "Editor"));
    options.AddPolicy("Administration", policy => policy.RequireAuthenticatedUser().RequireRole("SuperAdmin"));
});
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddAntiforgery(options => {
    options.Cookie.Name = "NLOIQ.Antiforgery";
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
});
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews().AddViewLocalization();
var app = builder.Build();
if (args.Contains("--create-admin", StringComparer.Ordinal))
{
    await using var scope = app.Services.CreateAsyncScope();
    await NLOIQ.Web.Security.AdminBootstrap.RunAsync(scope.ServiceProvider);
    return;
}
if (args.Contains("--inspect-content", StringComparer.Ordinal))
{
    if (!app.Environment.IsDevelopment()) throw new InvalidOperationException("Content inspection is available only in Development.");
    await using var scope = app.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<NloDbContext>();
    var items = await database.ContentItems.AsNoTracking().OrderBy(item => item.Kind).ThenBy(item => item.Key).Select(item => new { item.Kind, item.Key, item.State, item.IsDemo, item.DraftRevisionId, item.PublishedRevisionId }).ToListAsync();
    var report = new {
        AppliedMigrations = (await database.Database.GetAppliedMigrationsAsync()).ToArray(),
        Summary = new { ContentItems = items.Count, Revisions = await database.ContentRevisions.CountAsync(), Translations = await database.ContentTranslations.CountAsync(), MediaAssets = await database.MediaAssets.CountAsync(), Roles = await database.Roles.CountAsync(), Users = await database.Users.CountAsync(), Published = items.Count(item => item.State == NLOIQ.Domain.Content.PublicationState.Published) },
        Items = items.Select(item => new { Kind = item.Kind.ToString(), item.Key, State = item.State.ToString(), item.IsDemo, HasDraft = item.DraftRevisionId.HasValue, HasPublished = item.PublishedRevisionId.HasValue })
    };
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    return;
}
if (args.Contains("--initialize-database", StringComparer.Ordinal))
{
    if (!app.Environment.IsDevelopment()) throw new InvalidOperationException("Demo initialization is allowed only in Development.");
    await using var scope = app.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<NloDbContext>();
    await database.Database.MigrateAsync();
    var inserted = await scope.ServiceProvider.GetRequiredService<DevelopmentSeeder>().SeedAsync();
    var contentInserted = await scope.ServiceProvider.GetRequiredService<ContentArchitectureSeeder>()
        .SeedAsync(app.Environment.WebRootPath, builder.Configuration["Preview:VideoIds:network"]);
    Console.WriteLine($"Database initialized. Inserted {inserted} preview services and {contentInserted} draft content items; existing records preserved.");
    return;
}
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/error");
app.Use(async (context, next) => {
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; frame-src https://www.youtube-nocookie.com; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    if (app.Environment.IsDevelopment()) context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
    await next();
});
var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };
app.UseRouting();
app.UseRequestLocalization(new RequestLocalizationOptions {
    DefaultRequestCulture = new RequestCulture("en"), SupportedCultures = supportedCultures, SupportedUICultures = supportedCultures,
    RequestCultureProviders = [new Microsoft.AspNetCore.Localization.Routing.RouteDataRequestCultureProvider { RouteDataStringKey = "culture", UIRouteDataStringKey = "culture", Options = new RequestLocalizationOptions { SupportedCultures = supportedCultures, SupportedUICultures = supportedCultures } }]
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapGet("/", () => Results.Redirect("/en/"));
app.MapGet("/error", () => Results.Problem("The request could not be completed."));
app.MapControllerRoute("localized", "{culture:regex(^(en|ar)$)}/{controller=Home}/{action=Index}/{id?}");
app.Run();
public partial class Program { }
