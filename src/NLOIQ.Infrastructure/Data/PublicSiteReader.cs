using Microsoft.EntityFrameworkCore;
using NLOIQ.Application.Content;
using NLOIQ.Application.Interfaces;
using NLOIQ.Application.Validation;
using NLOIQ.Domain.Content;
using NLOIQ.Infrastructure.Media;
namespace NLOIQ.Infrastructure.Data;
public sealed class PublicSiteReader(NloDbContext db,IPublishedContentReader published):IPublicSiteReader
{
    private IQueryable<ContentItem> Eligible=>db.ContentItems.AsNoTracking().Where(x=>x.State==PublicationState.Published&&!x.IsDemo&&x.PublishedRevisionId!=null);
    private Task<List<T>> Children<T>(long revision,CancellationToken ct)where T:class,IRevisionOwned=>db.Set<T>().AsNoTracking().Where(x=>x.RevisionId==revision).ToListAsync(ct);
    private async Task<PublicNode?> Named(ContentKind kind,string key,string culture,CancellationToken ct) {
        var item=await Eligible.SingleOrDefaultAsync(x=>x.Kind==kind&&x.Key==key,ct);return item is null?null:await Node(item.Id,item.PublishedRevisionId!.Value,culture,ct);
    }
    private async Task<PublicNode?> Node(long owner,long revision,string culture,CancellationToken ct)
    {
        var item=await Eligible.SingleOrDefaultAsync(x=>x.Id==owner,ct);if(item is null)return null;
        if(!await db.ContentRevisions.AnyAsync(x=>x.Id==revision&&x.ContentItemId==owner,ct))return null;
        var text=await db.ContentTranslations.AsNoTracking().SingleOrDefaultAsync(x=>x.RevisionId==revision&&x.Culture==culture,ct);if(text is null)return null;
        var node=new PublicNode{Id=owner,RevisionId=revision,Kind=item.Kind,Key=item.Key,Title=text.Title,Summary=text.Summary,Body=text.Body,Slug=text.Slug,PrimaryLabel=text.PrimaryButtonLabel,SecondaryLabel=text.SecondaryButtonLabel};
        var section=(await Children<ContentSection>(revision,ct)).SingleOrDefault();if(section is not null){node.SectionKind=section.SectionKind;node.Layout=section.LayoutKey;node.Background=section.Background;node.Parallax=section.ParallaxIntensity;node.Visible=section.IsVisible;node.PrimaryTarget=section.PrimaryButtonTarget;node.SecondaryTarget=section.SecondaryButtonTarget;}
        var project=(await Children<ContentProject>(revision,ct)).SingleOrDefault();if(project is not null){node.VideoId=YouTubeVideoId.IsValid(project.YouTubeVideoId)?project.YouTubeVideoId:null;node.Featured=project.IsFeatured;node.Order=project.DisplayOrder;}
        var service=(await Children<ContentService>(revision,ct)).SingleOrDefault();if(service is not null){node.Icon=service.IconKey;node.Visible=service.IsVisible;node.Order=service.DisplayOrder;}
        var footer=(await Children<FooterSettings>(revision,ct)).SingleOrDefault();if(footer is not null)node.ShowCopyright=footer.ShowCopyright;
        var mediaText=await Children<RevisionMediaTranslation>(revision,ct);
        var imageUsages=new Dictionary<long,PublicImage>();
        foreach(var usage in (await Children<RevisionMedia>(revision,ct)).OrderBy(x=>x.UsageKey=="cover"?0:1).ThenBy(x=>x.DisplayOrder)) {
            var asset=await db.MediaAssets.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==usage.MediaAssetId&&!x.IsDemo,ct);var alt=mediaText.SingleOrDefault(x=>x.RevisionMediaId==usage.Id&&x.Culture==culture);
            if(asset is null||alt is null||string.IsNullOrWhiteSpace(alt.AltText))continue;
            var image=MediaAccess.Image(asset,alt.AltText,alt.Caption);if(image is null)continue;node.Images.Add(image);imageUsages[usage.Id]=image;
        }
        var seo=await db.Set<SeoMetadata>().AsNoTracking().SingleOrDefaultAsync(x=>x.RevisionId==revision&&x.Culture==culture,ct);
        if(seo is not null)node.Seo=new(seo.Title,seo.Description,seo.OpenGraphTitle,seo.OpenGraphDescription,seo.OpenGraphMediaUsageId is long imageId?imageUsages.GetValueOrDefault(imageId):null,seo.CanonicalPath,seo.NoIndex);
        var nav=(await Children<NavigationItem>(revision,ct)).OrderBy(x=>x.DisplayOrder).ToList();var navText=await Children<NavigationTranslation>(revision,ct);
        foreach(var link in nav.Where(x=>x.IsVisible)) {
            var label=navText.SingleOrDefault(x=>x.NavigationItemId==link.Id&&x.Culture==culture)?.Label;
            if(string.IsNullOrWhiteSpace(label)||string.IsNullOrWhiteSpace(link.Href)||!ContentRules.IsSafeTarget(link.Href))continue;
            var parent=link;var visible=true;var depth=0;var visited=new HashSet<long>();
            while(parent.ParentItemId is long parentId){if(!visited.Add(parentId)){visible=false;break;}var ancestor=nav.SingleOrDefault(x=>x.Id==parentId);if(ancestor is null||!ancestor.IsVisible){visible=false;break;}parent=ancestor;depth++;}
            if(visible)node.Links.Add(new(label,link.Href.Replace("{culture}",culture,StringComparison.Ordinal),depth));
        }
        var contactText=await Children<ContactTranslation>(revision,ct);
        foreach(var detail in (await Children<ContactDetail>(revision,ct)).OrderBy(x=>x.DisplayOrder)) {
            var label=contactText.SingleOrDefault(x=>x.ContactDetailId==detail.Id&&x.Culture==culture);if(label is null)continue;
            var href=detail.Kind switch{ContactKind.Email=>"mailto:"+detail.Value,ContactKind.Phone=>"tel:"+detail.Value,ContactKind.WhatsApp=>"https://wa.me/"+new string(detail.Value.Where(char.IsDigit).ToArray()),ContactKind.Map=>detail.Value,_=>null};
            node.Contacts.Add(new(label.Label,label.LocalizedValue??detail.Value,href));
        }
        foreach(var social in (await Children<SocialLink>(revision,ct)).OrderBy(x=>x.DisplayOrder))if(ContentRules.IsSafeTarget(social.Url))node.Social.Add(new(social.Platform,social.Url));
        foreach(var hours in (await Children<BusinessHours>(revision,ct)).OrderBy(x=>x.DayOfWeek))node.Hours.Add(new(hours.DayOfWeek,hours.IsClosed,hours.OpensAt,hours.ClosesAt));
        foreach(var category in await Children<ProjectCategoryAssignment>(revision,ct)) {
            var target=await db.ContentRevisions.AsNoTracking().SingleAsync(x=>x.Id==category.CategoryRevisionId,ct);
            if(await Eligible.AnyAsync(x=>x.Id==target.ContentItemId,ct)){var title=await db.ContentTranslations.AsNoTracking().Where(x=>x.RevisionId==target.Id&&x.Culture==culture).Select(x=>x.Title).SingleOrDefaultAsync(ct);if(title is not null)node.Categories.Add(title);}
        }
        return node;
    }
    public async Task<PublicSite?> ReadAsync(string culture,string? slug=null,bool project=false,CancellationToken ct=default)
    {
        if(!ContentRules.IsCulture(culture))return null;
        var found=slug is null?await published.FindAsync(ContentKind.Page,"home",culture,ct):await published.FindRouteAsync(project?"projects":"pages",slug,culture,ct);
        if(found is null)return null;
        var page=await Node(found.ContentItemId,found.RevisionId,culture,ct);if(page is null)return null;
        var sections=new List<PublicNode>();
        foreach(var link in (await Children<PageSection>(found.RevisionId,ct)).OrderBy(x=>x.DisplayOrder)) {
            var target=await db.ContentRevisions.AsNoTracking().SingleAsync(x=>x.Id==link.SectionRevisionId,ct);
            var section=await Node(target.ContentItemId,target.Id,culture,ct);if(section is {Visible:true})sections.Add(section);
        }
        var projects=new List<PublicNode>();var services=new List<PublicNode>();
        foreach(var item in await Eligible.Where(x=>x.Kind==ContentKind.Project||x.Kind==ContentKind.Service).ToListAsync(ct)) {
            var node=await Node(item.Id,item.PublishedRevisionId!.Value,culture,ct);if(node is not {Visible:true})continue;
            if(item.Kind==ContentKind.Project)projects.Add(node);else services.Add(node);
        }
        var alternate=culture=="en"?"ar":"en";
        var otherSlug=await db.ContentTranslations.AsNoTracking().Where(x=>x.RevisionId==page.RevisionId&&x.Culture==alternate).Select(x=>x.Slug).SingleOrDefaultAsync(ct);
        var alternateUrl=page.Kind==ContentKind.Page&&page.Key=="home"?$"/{alternate}/":$"/{alternate}/{(page.Kind==ContentKind.Project?"projects":"pages")}/{otherSlug}";
        return new(culture,page,sections,projects.OrderBy(x=>x.Order).ThenBy(x=>x.Id).ToList(),services.OrderBy(x=>x.Order).ThenBy(x=>x.Id).ToList(),
            await Named(ContentKind.Navigation,"main",culture,ct),await Named(ContentKind.Navigation,"footer",culture,ct),await Named(ContentKind.Contact,"primary",culture,ct),await Named(ContentKind.Footer,"main",culture,ct),alternateUrl);
    }
}
