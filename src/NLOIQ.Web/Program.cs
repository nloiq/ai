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
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews().AddViewLocalization();
var app = builder.Build();
if (args.Contains("--initialize-database", StringComparer.Ordinal))
{
    if (!app.Environment.IsDevelopment()) throw new InvalidOperationException("Demo initialization is allowed only in Development.");
    await using var scope = app.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<NloDbContext>();
    await database.Database.MigrateAsync();
    var inserted = await scope.ServiceProvider.GetRequiredService<DevelopmentSeeder>().SeedAsync();
    Console.WriteLine($"Database initialized. Inserted {inserted} demo services; existing records preserved.");
    return;
}
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/error");
app.Use(async (context, next) => {
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    if (app.Environment.IsDevelopment()) context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
    await next();
});
var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };
app.UseRouting();
app.UseRequestLocalization(new RequestLocalizationOptions {
    DefaultRequestCulture = new RequestCulture("en"), SupportedCultures = supportedCultures, SupportedUICultures = supportedCultures,
    RequestCultureProviders = [new Microsoft.AspNetCore.Localization.Routing.RouteDataRequestCultureProvider { RouteDataStringKey = "culture", UIRouteDataStringKey = "culture", Options = new RequestLocalizationOptions { SupportedCultures = supportedCultures, SupportedUICultures = supportedCultures } }]
});
app.UseStaticFiles();
app.MapGet("/", () => Results.Redirect("/en/"));
app.MapGet("/error", () => Results.Problem("The request could not be completed."));
app.MapControllerRoute("localized", "{culture:regex(^(en|ar)$)}/{controller=Home}/{action=Index}/{id?}");
app.Run();
public partial class Program { }
