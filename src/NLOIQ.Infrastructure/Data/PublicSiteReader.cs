using Microsoft.EntityFrameworkCore;
using NLOIQ.Application.Content;
using NLOIQ.Application.Interfaces;
using NLOIQ.Application.Validation;
using NLOIQ.Domain.Content;
using NLOIQ.Infrastructure.Media;
namespace NLOIQ.Infrastructure.Data;
public sealed class PublicSiteReader(NloDbContext db,IPublishedContentReader published):IPublicSiteReader,IContentPreviewReader
{
    private readonly Dictionary<long,MediaAsset?> assets=[];
    private IQueryable<ContentItem> Eligible=>db.ContentItems.AsNoTracking().Where(x=>x.State==PublicationState.Published&&!x.IsDemo&&x.PublishedRevisionId!=null);
    private Task<List<T>> Children<T>(long revision,CancellationToken ct)where T:class,IRevisionOwned=>db.Set<T>().AsNoTracking().Where(x=>x.RevisionId==revision).ToListAsync(ct);
    private async Task<PublicNode?> Named(ContentKind kind,string key,string culture,CancellationToken ct) {
        var item=await Eligible.SingleOrDefaultAsync(x=>x.Kind==kind&&x.Key==key,ct);return item is null?null:await Node(item.Id,item.PublishedRevisionId!.Value,culture,ct);
    }
    private async Task<PublicNode?> Node(long owner,long revision,string culture,CancellationToken ct,bool preview=false)
    {
        var item=await (preview?db.ContentItems.AsNoTracking():Eligible).SingleOrDefaultAsync(x=>x.Id==owner,ct);if(item is null)return null;
        if(!await db.ContentRevisions.AnyAsync(x=>x.Id==revision&&x.ContentItemId==owner,ct))return null;
        var text=await db.ContentTranslations.AsNoTracking().SingleOrDefaultAsync(x=>x.RevisionId==revision&&x.Culture==culture,ct);if(text is null)return null;
        var node=new PublicNode{Id=owner,RevisionId=revision,Kind=item.Kind,Key=item.Key,Title=text.Title,Summary=text.Summary,Body=text.Body,Slug=text.Slug,PrimaryLabel=text.PrimaryButtonLabel,SecondaryLabel=text.SecondaryButtonLabel};
        var section=item.Kind==ContentKind.Section?(await Children<ContentSection>(revision,ct)).SingleOrDefault():null;if(section is not null){node.SectionKind=section.SectionKind;node.Layout=section.LayoutKey;node.Background=section.Background;node.Parallax=section.ParallaxIntensity;node.Visible=section.IsVisible;node.PrimaryTarget=section.PrimaryButtonTarget;node.SecondaryTarget=section.SecondaryButtonTarget;}
        var project=item.Kind==ContentKind.Project?(await Children<ContentProject>(revision,ct)).SingleOrDefault():null;if(project is not null){node.VideoId=YouTubeVideoId.IsValid(project.YouTubeVideoId)?project.YouTubeVideoId:null;node.Featured=project.IsFeatured;node.Order=project.DisplayOrder;}
        var service=item.Kind==ContentKind.Service?(await Children<ContentService>(revision,ct)).SingleOrDefault():null;if(service is not null){node.Icon=service.IconKey;node.Visible=service.IsVisible;node.Order=service.DisplayOrder;}
        var footer=item.Kind==ContentKind.Footer?(await Children<FooterSettings>(revision,ct)).SingleOrDefault():null;if(footer is not null)node.ShowCopyright=footer.ShowCopyright;
        var usages=await Children<RevisionMedia>(revision,ct);
        var mediaText=usages.Count==0?[]:await Children<RevisionMediaTranslation>(revision,ct);
        var imageUsages=new Dictionary<long,PublicImage>();
        foreach(var usage in usages.OrderBy(x=>x.UsageKey=="cover"?0:1).ThenBy(x=>x.DisplayOrder)) {
            if(!assets.TryGetValue(usage.MediaAssetId,out var asset)){asset=await db.MediaAssets.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==usage.MediaAssetId,ct);assets[usage.MediaAssetId]=asset;}var alt=mediaText.SingleOrDefault(x=>x.RevisionMediaId==usage.Id&&x.Culture==culture);
            if(asset is null||(!preview&&asset.IsDemo)||alt is null||string.IsNullOrWhiteSpace(alt.AltText))continue;
            var image=MediaAccess.Image(asset,alt.AltText,alt.Caption,preview);if(image is null)continue;node.Images.Add(image);imageUsages[usage.Id]=image;
        }
        var seo=item.Kind is ContentKind.Page or ContentKind.Project?await db.Set<SeoMetadata>().AsNoTracking().SingleOrDefaultAsync(x=>x.RevisionId==revision&&x.Culture==culture,ct):null;
        if(seo is not null)node.Seo=new(seo.Title,seo.Description,seo.OpenGraphTitle,seo.OpenGraphDescription,seo.OpenGraphMediaUsageId is long imageId?imageUsages.GetValueOrDefault(imageId):null,seo.CanonicalPath,seo.NoIndex);
        var nav=item.Kind==ContentKind.Navigation?(await Children<NavigationItem>(revision,ct)).OrderBy(x=>x.DisplayOrder).ToList():[];var navText=nav.Count==0?[]:await Children<NavigationTranslation>(revision,ct);
        foreach(var link in nav.Where(x=>x.IsVisible)) {
            var label=navText.SingleOrDefault(x=>x.NavigationItemId==link.Id&&x.Culture==culture)?.Label;
            if(string.IsNullOrWhiteSpace(label)||string.IsNullOrWhiteSpace(link.Href)||!ContentRules.IsSafeTarget(link.Href))continue;
            var parent=link;var visible=true;var depth=0;var visited=new HashSet<long>();
            while(parent.ParentItemId is long parentId){if(!visited.Add(parentId)){visible=false;break;}var ancestor=nav.SingleOrDefault(x=>x.Id==parentId);if(ancestor is null||!ancestor.IsVisible){visible=false;break;}parent=ancestor;depth++;}
            if(visible)node.Links.Add(new(label,link.Href.Replace("{culture}",culture,StringComparison.Ordinal),depth));
        }
        var contactText=item.Kind==ContentKind.Contact?await Children<ContactTranslation>(revision,ct):[];
        foreach(var detail in (item.Kind==ContentKind.Contact?await Children<ContactDetail>(revision,ct):[]).OrderBy(x=>x.DisplayOrder)) {
            var label=contactText.SingleOrDefault(x=>x.ContactDetailId==detail.Id&&x.Culture==culture);if(label is null)continue;
            var href=detail.Kind switch{ContactKind.Email=>"mailto:"+detail.Value,ContactKind.Phone=>"tel:"+detail.Value,ContactKind.WhatsApp=>"https://wa.me/"+new string(detail.Value.Where(char.IsDigit).ToArray()),ContactKind.Map=>detail.Value,_=>null};
            node.Contacts.Add(new(label.Label,label.LocalizedValue??detail.Value,href));
        }
        foreach(var social in (item.Kind is ContentKind.Contact or ContentKind.Footer or ContentKind.SiteSettings?await Children<SocialLink>(revision,ct):[]).OrderBy(x=>x.DisplayOrder))if(ContentRules.IsSafeTarget(social.Url))node.Social.Add(new(social.Platform,social.Url));
        foreach(var hours in (item.Kind==ContentKind.Contact?await Children<BusinessHours>(revision,ct):[]).OrderBy(x=>x.DayOfWeek))node.Hours.Add(new(hours.DayOfWeek,hours.IsClosed,hours.OpensAt,hours.ClosesAt));
        foreach(var category in item.Kind==ContentKind.Project?await Children<ProjectCategoryAssignment>(revision,ct):[]) {
            var target=await db.ContentRevisions.AsNoTracking().SingleAsync(x=>x.Id==category.CategoryRevisionId,ct);
            if(preview||await Eligible.AnyAsync(x=>x.Id==target.ContentItemId,ct)){var title=await db.ContentTranslations.AsNoTracking().Where(x=>x.RevisionId==target.Id&&x.Culture==culture).Select(x=>x.Title).SingleOrDefaultAsync(ct);if(title is not null)node.Categories.Add(title);}
        }
        return node;
    }
    public async Task<PublicSite?> ReadAsync(string culture,string? slug=null,bool project=false,CancellationToken ct=default)
    {
        assets.Clear();
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
    public async Task<PublicSite?> PreviewAsync(long id,long revisionId,string culture,CancellationToken ct=default)
    {
        assets.Clear();
        if(!ContentRules.IsCulture(culture))return null;
        var page=await Node(id,revisionId,culture,ct,true);if(page is null)return null;
        var sections=new List<PublicNode>();
        foreach(var link in (await Children<PageSection>(revisionId,ct)).OrderBy(x=>x.DisplayOrder)) {
            var target=await db.ContentRevisions.AsNoTracking().SingleAsync(x=>x.Id==link.SectionRevisionId,ct);
            var block=await Node(target.ContentItemId,target.Id,culture,ct,true);if(block is {Visible:true})sections.Add(block);
        }
        if(page.Kind==ContentKind.Section)sections.Add(page);
        var projects=new List<PublicNode>();var services=new List<PublicNode>();
        foreach(var item in await Eligible.Where(x=>x.Kind==ContentKind.Project||x.Kind==ContentKind.Service).ToListAsync(ct)) {
            var node=await Node(item.Id,item.PublishedRevisionId!.Value,culture,ct);if(node is not {Visible:true})continue;
            if(item.Kind==ContentKind.Project)projects.Add(node);else services.Add(node);
        }
        if(page.Kind==ContentKind.Project){projects.RemoveAll(x=>x.Id==id);projects.Add(page);}
        if(page.Kind==ContentKind.Service){services.RemoveAll(x=>x.Id==id);services.Add(page);}
        void PrivateImages(PublicNode? node){if(node is null)return;PublicImage Convert(PublicImage image){if(!image.Url.StartsWith("/media/",StringComparison.Ordinal))return image;var asset=image.Url.Split('/')[2];var url=$"/{culture}/admin/content/{node.Id}/revisions/{node.RevisionId}/media/{asset}/full";return image with{Url=url,SrcSet=null};}
            for(var index=0;index<node.Images.Count;index++)node.Images[index]=Convert(node.Images[index]);
            if(node.Seo?.Image is { } image)node.Seo=node.Seo with{Image=Convert(image)};
        }
        var navigation=page.Kind==ContentKind.Navigation?page:await Named(ContentKind.Navigation,"main",culture,ct);
        var footerNav=await Named(ContentKind.Navigation,"footer",culture,ct);
        if(page.Kind==ContentKind.Navigation&&page.Key=="footer"){footerNav=page;navigation=await Named(ContentKind.Navigation,"main",culture,ct);}
        var contact=page.Kind==ContentKind.Contact?page:await Named(ContentKind.Contact,"primary",culture,ct);
        var footer=page.Kind==ContentKind.Footer?page:await Named(ContentKind.Footer,"main",culture,ct);
        foreach(var node in new[]{page,navigation,footerNav,contact,footer}.Concat(sections).Concat(projects).Concat(services).DistinctBy(x=>x?.RevisionId))PrivateImages(node);
        var alternate=culture=="en"?"ar":"en";
        return new(culture,page,sections,projects.OrderBy(x=>x.Order).ToList(),services.OrderBy(x=>x.Order).ToList(),navigation,footerNav,contact,footer,$"/{alternate}/admin/content/{id}/revisions/{revisionId}/preview",true);
    }

}
