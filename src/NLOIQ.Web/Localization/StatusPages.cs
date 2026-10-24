using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Localization;
namespace NLOIQ.Web.Localization;
public static class StatusPages
{
    public static async Task WriteAsync(StatusCodeContext status)
    {
        var context=status.HttpContext;
        if(!context.Request.Headers.Accept.Any(value=>value?.Contains("text/html",StringComparison.OrdinalIgnoreCase)==true))return;
        var culture=SiteLanguage.Supported(context.Request.Path.Value?.Split('/',StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())??SiteLanguage.Supported(context.Request.RouteValues["culture"]?.ToString())??SiteLanguage.Supported(context.Request.Cookies[SiteLanguage.CookieName])??"en";
        System.Globalization.CultureInfo.CurrentUICulture=System.Globalization.CultureInfo.GetCultureInfo(culture);
        var text=context.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();
        var key=context.Response.StatusCode switch{404=>"StatusNotFound",429=>"StatusRateLimited",403=>"StatusForbidden",_=>"RequestFailed"};
        string Encode(string value)=>HtmlEncoder.Default.Encode(value);
        var title=Encode(text[key]);var help=Encode(text["StatusHelp"]);var home=Encode(text["AdminBack"]);
        context.Response.ContentType="text/html; charset=utf-8";context.Response.Headers.CacheControl="no-store";context.Response.Headers["X-Robots-Tag"]="noindex, nofollow";context.Response.Headers["Content-Language"]=culture;
        await context.Response.WriteAsync($"<!doctype html><html lang=\"{culture}\" dir=\"{(culture=="ar"?"rtl":"ltr")}\" data-default-theme=\"system\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><meta name=\"robots\" content=\"noindex,nofollow\"><title>{title} · NLO IQ</title><script src=\"/js/theme-init.js\"></script><link rel=\"stylesheet\" href=\"/css/app.css\"></head><body class=\"min-h-screen bg-paper text-ink\"><main class=\"mx-auto max-w-3xl px-5 py-16\"><p class=\"text-sm text-signal\" dir=\"ltr\">NLO IQ / {context.Response.StatusCode}</p><h1 class=\"my-6 text-3xl\">{title}</h1><p class=\"leading-7 text-muted\">{help}</p><a class=\"mt-6 inline-flex min-h-11 items-center underline\" href=\"/{culture}/\">{home}</a></main></body></html>");
    }
}
