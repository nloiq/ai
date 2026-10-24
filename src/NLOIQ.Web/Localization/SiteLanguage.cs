using Microsoft.AspNetCore.WebUtilities;
namespace NLOIQ.Web.Localization;
public static class SiteLanguage
{
    public const string CookieName = "NLOIQ.Language";
    public static string? Supported(string? value) => value is "en" or "ar" ? value : null;
    public static string SwitchAdmin(HttpRequest request, string culture)
    {
        var path = request.Path.Value ?? "/en/";
        var target = "/" + culture + path[3..];
        var query = QueryHelpers.ParseQuery(request.QueryString.Value);
        if (query.TryGetValue("returnUrl", out var value)) {
            var local = value.ToString();
            if (local.StartsWith("/en/", StringComparison.Ordinal) || local.StartsWith("/ar/", StringComparison.Ordinal))
                query["returnUrl"] = "/" + culture + local[3..];
        }
        return QueryHelpers.AddQueryString(target, query);
    }
}
