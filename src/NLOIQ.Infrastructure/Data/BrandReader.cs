using Microsoft.EntityFrameworkCore;
using NLOIQ.Application.Content;
using NLOIQ.Application.Validation;
using NLOIQ.Domain.Content;
using NLOIQ.Infrastructure.Media;
namespace NLOIQ.Infrastructure.Data;
public sealed class BrandReader(NloDbContext db):IBrandReader
{
    private IQueryable<ContentItem> Eligible=>db.ContentItems.AsNoTracking().Where(x=>x.State==PublicationState.Published&&!x.IsDemo&&x.PublishedRevisionId!=null);
    public async Task<PublicBrand> ReadAsync(string culture,CancellationToken ct=default)
    {
        var root=await Eligible.SingleOrDefaultAsync(x=>x.Kind==ContentKind.SiteSettings&&x.Key=="site",ct);var theme=await Eligible.Where(x=>x.Kind==ContentKind.ThemeSettings&&x.Key=="brand").Select(x=>x.PublishedRevisionId).SingleOrDefaultAsync(ct);
        var site=root is null?null:await db.Set<SiteSettings>().AsNoTracking().SingleOrDefaultAsync(x=>x.RevisionId==root.PublishedRevisionId,ct);
        async Task<PublicImage?> Image(long? usageId){if(usageId is null)return null;var usage=await db.Set<RevisionMedia>().AsNoTracking().Include(x=>x.MediaAsset).SingleOrDefaultAsync(x=>x.Id==usageId&&x.RevisionId==site!.RevisionId,ct);if(usage is null)return null;var alt=await db.Set<RevisionMediaTranslation>().AsNoTracking().Where(x=>x.RevisionMediaId==usage.Id&&x.Culture==culture).Select(x=>x.AltText).SingleOrDefaultAsync(ct);return alt is null?null:MediaAccess.Image(usage.MediaAsset,alt);}
        var logo=await Image(site?.LogoMediaUsageId);var icon=await Image(site?.FaviconMediaUsageId);string? favicon=icon?.Url;if(icon is not null&&icon.Url.StartsWith("/media/",StringComparison.Ordinal))favicon=icon.Url[..^4]+"icon";
        var social=site is null?[]:await db.Set<SocialLink>().AsNoTracking().Where(x=>x.RevisionId==site.RevisionId).OrderBy(x=>x.DisplayOrder).Select(x=>new PublicLink(x.Platform,x.Url)).ToListAsync(ct);
        var selectorStyle=theme is long themeId?await db.Set<ThemeSettings>().AsNoTracking().Where(x=>x.RevisionId==themeId).Select(x=>x.ThemeSelectorStyle).SingleAsync(ct):0;
        return new PublicBrand(site?.SiteName??"NLO IQ",site?.DefaultCulture??"en",site?.DefaultTheme??"system",site?.PublicBaseUrl,logo,favicon,theme,social.Where(x=>ContentRules.IsSafeTarget(x.Href)).ToList()) { ThemeSelectorStyle=selectorStyle };
    }
    public async Task<string?> ThemeCssAsync(long revision,CancellationToken ct=default)
    {
        if(!await Eligible.AnyAsync(x=>x.Kind==ContentKind.ThemeSettings&&x.Key=="brand"&&x.PublishedRevisionId==revision,ct))return null;
        var theme=await db.Set<ThemeSettings>().AsNoTracking().SingleAsync(x=>x.RevisionId==revision,ct);ContentRules.Validate(theme);
        var lightLine=ContentRules.Contrast(theme.Moss,theme.Ivory)>=3?theme.Moss:theme.Evergreen;
        var darkLine=Math.Min(ContentRules.Contrast(theme.Moss,theme.Evergreen),ContentRules.Contrast(theme.Moss,theme.Charcoal))>=3?theme.Moss:theme.Gold;
        return $":root{{--logo-border-width:{(theme.LogoBorderEnabled?1:0)}px;--logo-border-color:{theme.LogoBorderColor};--logo-radius:{theme.LogoRadius}px;--logo-height:{theme.LogoHeight}px;--theme-selector-radius:{theme.ThemeSelectorRadius}px;--evergreen:{theme.Evergreen};--forest:{theme.Forest};--moss:{theme.Moss};--burnt-orange:{theme.BurntOrange};--charcoal:{theme.Charcoal};--ivory:{theme.Ivory};--copper:{theme.Copper};--gold:{theme.Gold};--paper:{theme.Ivory};--ink:{theme.Evergreen};--muted:{theme.Forest};--line:{lightLine};--panel:color-mix(in srgb,{theme.Ivory} 90%,{theme.Moss});--accent:{theme.LightAction};--on-accent:{theme.Ivory};--signal:{theme.SignalLight};}}:root[data-theme=dark]{{--paper:{theme.Evergreen};--ink:{theme.Ivory};--muted:color-mix(in srgb,{theme.Ivory} 85%,{theme.Moss});--line:{darkLine};--panel:{theme.Charcoal};--accent:{theme.Gold};--on-accent:{theme.Evergreen};--signal:{theme.SignalDark};}}";
    }
}
