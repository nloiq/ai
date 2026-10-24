using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NLOIQ.Application.Content;
using NLOIQ.Application.Validation;
using NLOIQ.Domain.Content;
namespace NLOIQ.Infrastructure.Data;
public sealed class ContentEditor(NloDbContext db) : IContentEditor
{
    private static readonly ContentKind[] Supported = [ContentKind.Page,ContentKind.Section,ContentKind.Project,ContentKind.ProjectCategory,ContentKind.Service,ContentKind.Navigation,ContentKind.Contact,ContentKind.Footer,ContentKind.SiteSettings,ContentKind.ThemeSettings];
    public async Task<IReadOnlyList<ContentListRow>> ListAsync(ContentKind? kind,string? search,CancellationToken ct=default,string culture="en")
    {
        var query=db.ContentItems.AsNoTracking().Where(x=>Supported.Contains(x.Kind));
        if(kind.HasValue) query=query.Where(x=>x.Kind==kind);
        if(!string.IsNullOrWhiteSpace(search)) query=query.Where(x=>x.Key.Contains(search));
        return await query.OrderBy(x=>x.Kind).ThenBy(x=>x.Key).Select(x=>new ContentListRow(x.Id,x.Kind,x.Key,x.State,x.IsDemo,x.Version,
            x.DraftRevision==null?0:x.DraftRevision.RevisionNumber,
            db.ContentTranslations.Where(t=>t.RevisionId==x.DraftRevisionId && t.Culture==culture).Select(t=>t.Title).FirstOrDefault()??x.Key,db.Set<ContentSection>().Where(section=>section.RevisionId==x.DraftRevisionId).Select(section=>(SectionKind?)section.SectionKind).FirstOrDefault())).ToListAsync(ct);
    }
    private Task<List<T>> Children<T>(long revisionId,CancellationToken ct) where T:class,IRevisionOwned => db.Set<T>().AsNoTracking().Where(x=>x.RevisionId==revisionId).ToListAsync(ct);
    public async Task<EditorChoices> ChoicesAsync(CancellationToken ct=default,string culture="en") => new(
        await db.ContentItems.AsNoTracking().Where(x=>x.Kind==ContentKind.Section && x.State!=PublicationState.Archived).OrderBy(x=>x.Key).Select(x=>new ContentChoice(x.Id,db.ContentTranslations.Where(t=>t.RevisionId==x.DraftRevisionId&&t.Culture==culture).Select(t=>t.Title).FirstOrDefault()??x.Key)).ToListAsync(ct),
        await db.ContentItems.AsNoTracking().Where(x=>x.Kind==ContentKind.ProjectCategory && x.State!=PublicationState.Archived).OrderBy(x=>x.Key).Select(x=>new ContentChoice(x.Id,db.ContentTranslations.Where(t=>t.RevisionId==x.DraftRevisionId&&t.Culture==culture).Select(t=>t.Title).FirstOrDefault()??x.Key)).ToListAsync(ct),
        await db.MediaAssets.AsNoTracking().OrderBy(x=>x.Id).Select(x=>new MediaChoice(x.Id,x.OriginalFileName,x.IsDemo,db.Set<MediaAssetTranslation>().Where(t=>t.MediaAssetId==x.Id&&t.Culture=="en").Select(t=>t.AltText).FirstOrDefault()??"",db.Set<MediaAssetTranslation>().Where(t=>t.MediaAssetId==x.Id&&t.Culture=="ar").Select(t=>t.AltText).FirstOrDefault()??"")).ToListAsync(ct));
    public async Task<ContentEdit?> GetAsync(long id,CancellationToken ct=default)
    {
        var item=await db.ContentItems.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);
        if(item is null || !Supported.Contains(item.Kind))return null;
        var model=new ContentEdit{Id=id,Version=item.Version,Kind=item.Kind,Key=item.Key};
        if(item.DraftRevisionId is not long revision)return model;
        foreach(var t in await Children<ContentTranslation>(revision,ct)) {
            var text=new TranslationEdit{Title=t.Title,Summary=t.Summary,Body=t.Body,Slug=t.Slug,PrimaryButtonLabel=t.PrimaryButtonLabel,SecondaryButtonLabel=t.SecondaryButtonLabel};
            if(t.Culture=="en")model.English=text;else if(t.Culture=="ar")model.Arabic=text;
        }
        var section=(await Children<ContentSection>(revision,ct)).SingleOrDefault();
        if(section is not null){model.SectionKind=section.SectionKind;model.LayoutKey=section.LayoutKey;model.Background=section.Background;model.IsVisible=section.IsVisible;model.ParallaxIntensity=section.ParallaxIntensity;model.PrimaryButtonTarget=section.PrimaryButtonTarget;model.SecondaryButtonTarget=section.SecondaryButtonTarget;}
        var project=(await Children<ContentProject>(revision,ct)).SingleOrDefault();
        if(project is not null){model.YouTubeVideoId=project.YouTubeVideoId;model.DisplayOrder=project.DisplayOrder;model.IsFeatured=project.IsFeatured;}
        var service=(await Children<ContentService>(revision,ct)).SingleOrDefault();
        if(service is not null){model.IconKey=service.IconKey;model.DisplayOrder=service.DisplayOrder;model.IsVisible=service.IsVisible;}
        var footer=(await Children<FooterSettings>(revision,ct)).SingleOrDefault();if(footer is not null)model.ShowCopyright=footer.ShowCopyright;
        model.Sections=await db.Set<PageSection>().AsNoTracking().Where(x=>x.RevisionId==revision).OrderBy(x=>x.DisplayOrder).Select(x=>new SectionEdit{ContentItemId=x.Section.Revision.ContentItemId,DisplayOrder=x.DisplayOrder}).ToListAsync(ct);
        model.Categories=await db.Set<ProjectCategoryAssignment>().AsNoTracking().Where(x=>x.RevisionId==revision).Select(x=>x.Category.Revision.ContentItemId).ToListAsync(ct);
        var site=(await Children<SiteSettings>(revision,ct)).SingleOrDefault();
        if(site is not null)model.Site=new(){SiteName=site.SiteName,DefaultCulture=site.DefaultCulture,DefaultTheme=site.DefaultTheme,PublicBaseUrl=site.PublicBaseUrl};
        var theme=(await Children<ThemeSettings>(revision,ct)).SingleOrDefault();
        if(theme is not null)foreach(var property in typeof(ThemeEdit).GetProperties())property.SetValue(model.Theme,typeof(ThemeSettings).GetProperty(property.Name)!.GetValue(theme));
        var seoUsages=(await Children<RevisionMedia>(revision,ct)).ToDictionary(x=>x.Id,x=>x.UsageKey);
        foreach(var seo in await Children<SeoMetadata>(revision,ct)){
            var edit=new SeoEdit{Title=seo.Title,Description=seo.Description,OpenGraphTitle=seo.OpenGraphTitle,OpenGraphDescription=seo.OpenGraphDescription,CanonicalPath=seo.CanonicalPath,NoIndex=seo.NoIndex,OpenGraphUsageKey=seo.OpenGraphMediaUsageId is long usage?seoUsages.GetValueOrDefault(usage):null};
            if(seo.Culture=="en")model.SeoEnglish=edit;else model.SeoArabic=edit;
        }
        var mediaText=await Children<RevisionMediaTranslation>(revision,ct);
        foreach(var media in (await Children<RevisionMedia>(revision,ct)).OrderBy(x=>x.DisplayOrder)) {
            var en=mediaText.SingleOrDefault(x=>x.RevisionMediaId==media.Id&&x.Culture=="en");var ar=mediaText.SingleOrDefault(x=>x.RevisionMediaId==media.Id&&x.Culture=="ar");
            model.Media.Add(new(){AssetId=media.MediaAssetId,UsageKey=media.UsageKey,AltEn=en?.AltText??"",AltAr=ar?.AltText??"",CaptionEn=en?.Caption,CaptionAr=ar?.Caption});
        }
        if(site is not null){var logo=model.Media.SingleOrDefault(x=>x.UsageKey=="logo");var icon=model.Media.SingleOrDefault(x=>x.UsageKey=="favicon");model.Site.LogoAssetId=logo?.AssetId;model.Site.LogoAltEn=logo?.AltEn??"";model.Site.LogoAltAr=logo?.AltAr??"";model.Site.FaviconAssetId=icon?.AssetId;}
        var links=(await Children<NavigationItem>(revision,ct)).OrderBy(x=>x.DisplayOrder).ToList();var labels=await Children<NavigationTranslation>(revision,ct);
        foreach(var link in links)model.Links.Add(new(){Href=link.Href??"",IsVisible=link.IsVisible,ParentIndex=link.ParentItemId is long parent?links.FindIndex(x=>x.Id==parent):null,
            LabelEn=labels.SingleOrDefault(x=>x.NavigationItemId==link.Id&&x.Culture=="en")?.Label??"",LabelAr=labels.SingleOrDefault(x=>x.NavigationItemId==link.Id&&x.Culture=="ar")?.Label??""});
        var contactText=await Children<ContactTranslation>(revision,ct);
        foreach(var contact in (await Children<ContactDetail>(revision,ct)).OrderBy(x=>x.DisplayOrder)) {
            var en=contactText.SingleOrDefault(x=>x.ContactDetailId==contact.Id&&x.Culture=="en");var ar=contactText.SingleOrDefault(x=>x.ContactDetailId==contact.Id&&x.Culture=="ar");
            model.Contacts.Add(new(){Kind=contact.Kind,Value=contact.Value,LabelEn=en?.Label??"",LabelAr=ar?.Label??"",ValueEn=en?.LocalizedValue,ValueAr=ar?.LocalizedValue});
        }
        model.Social=(await Children<SocialLink>(revision,ct)).OrderBy(x=>x.DisplayOrder).Select(x=>new SocialEdit{Platform=x.Platform,Url=x.Url}).ToList();
        model.Hours=(await Children<BusinessHours>(revision,ct)).OrderBy(x=>x.DayOfWeek).Select(x=>new HoursEdit{DayOfWeek=x.DayOfWeek,IsClosed=x.IsClosed,OpensAt=x.OpensAt,ClosesAt=x.ClosesAt}).ToList();return model;
    }
    private static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
    private static void ValidateModel(ContentEdit m)
    {
        Require(Supported.Contains(m.Kind),"Unsupported content module.");
        Validator.ValidateObject(m,new ValidationContext(m),true);
        foreach(var value in new object[]{m.English,m.Arabic,m.SeoEnglish,m.SeoArabic,m.Site,m.Theme}.Concat(m.Media).Concat(m.Links).Concat(m.Contacts).Concat(m.Social).Concat(m.Hours))Validator.ValidateObject(value,new ValidationContext(value),true);
        Require(m.Sections.Count<=50&&m.Categories.Count<=50&&m.Media.Count<=30&&m.Links.Count<=50&&m.Contacts.Count<=30&&m.Social.Count<=5&&m.Hours.Count<=7,"Too many collection rows.");
        Require(m.Sections.Select(x=>x.ContentItemId).Distinct().Count()==m.Sections.Count,"A section may only appear once on a page.");
        Require(m.Categories.Distinct().Count()==m.Categories.Count,"Duplicate project categories.");
        Require(m.Media.Select(x=>x.UsageKey).Distinct().Count()==m.Media.Count,"Image usage names must be unique.");
        Require(m.Social.Select(x=>x.Platform).Distinct().Count()==m.Social.Count,"Duplicate social platforms.");
        Require(m.Hours.Select(x=>x.DayOfWeek).Distinct().Count()==m.Hours.Count,"Duplicate business-hour days.");
        foreach(var h in m.Hours)Require(h.IsClosed || (h.OpensAt.HasValue&&h.ClosesAt.HasValue&&h.OpensAt<h.ClosesAt),"Opening and closing times must form a valid same-day interval.");
        for(var i=0;i<m.Links.Count;i++) {
            Require(!string.IsNullOrWhiteSpace(m.Links[i].Href)&&ContentRules.IsSafeTarget(m.Links[i].Href),"Each navigation link requires a safe target.");
            Require(m.Links[i].ParentIndex is null || m.Links[i].ParentIndex>=0&&m.Links[i].ParentIndex<i,"Navigation parents must precede their children.");
        }
        if(m.Kind==ContentKind.Navigation)Require(m.Key is "main" or "footer","Navigation key must be main or footer.");
        if(m.Kind==ContentKind.Contact)Require(m.Key=="primary","Contact key must be primary.");
        if(m.Kind==ContentKind.SiteSettings)Require(m.Key=="site","Site settings key must be site.");
        if(m.Kind==ContentKind.ThemeSettings)Require(m.Key=="brand","Theme settings key must be brand.");
        if(m.Kind==ContentKind.Footer)Require(m.Key=="main","Footer key must be main.");
    }
    private async Task<long> Dependency(long id,ContentKind kind,CancellationToken ct)
    {
        var item=await db.ContentItems.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id&&x.Kind==kind&&x.State!=PublicationState.Archived,ct);
        return item?.PublishedRevisionId??item?.DraftRevisionId??throw new InvalidOperationException("A selected dependency is missing or archived.");
    }
    public async Task<long> SaveAsync(ContentEdit m,Guid actorId,string actorLabel,CancellationToken ct=default)
    {
        actorLabel=actorLabel[..Math.Min(actorLabel.Length,160)];
        if(m.Kind==ContentKind.SiteSettings){
            m.Media=[];
            if(m.Site.LogoAssetId is long logo)m.Media.Add(new(){AssetId=logo,UsageKey="logo",AltEn=m.Site.LogoAltEn,AltAr=m.Site.LogoAltAr});
            if(m.Site.FaviconAssetId is long icon)m.Media.Add(new(){AssetId=icon,UsageKey="favicon",AltEn=m.Site.SiteName,AltAr=m.Site.SiteName});
        }
        ValidateModel(m);
        await using var tx=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;
        try {
            var item=m.Id==0?new ContentItem{Kind=m.Kind,Key=m.Key,IsDemo=true}:await db.ContentItems.SingleOrDefaultAsync(x=>x.Id==m.Id,ct)??throw new KeyNotFoundException();
            Require(item.Kind==m.Kind&&item.Key==m.Key,"Existing content keys and types cannot be changed.");
            if(m.Id!=0&&item.Version!=m.Version)throw new ContentConflictException();
            var previous=item.DraftRevisionId;
            var revision=new ContentRevision{ContentItem=item,RevisionNumber=await db.ContentRevisions.Where(x=>x.ContentItemId==item.Id).Select(x=>(int?)x.RevisionNumber).MaxAsync(ct) is int number?number+1:1,
                SupersedesRevisionId=previous,CreatedByUserId=actorId,ChangeNote=m.ChangeNote??""};
            db.Add(revision);
            foreach(var (culture,t) in new[]{("en",m.English),("ar",m.Arabic)})revision.Translations.Add(new(){Revision=revision,Culture=culture,Title=t.Title??"",Summary=t.Summary??"",Body=t.Body??"",Slug=string.IsNullOrWhiteSpace(t.Slug)?null:t.Slug,PrimaryButtonLabel=t.PrimaryButtonLabel,SecondaryButtonLabel=t.SecondaryButtonLabel});
            var usages=new Dictionary<string,RevisionMedia>();
            foreach(var (media,index) in m.Media.Select((x,i)=>(x,i))) {
                Require(await db.MediaAssets.AnyAsync(x=>x.Id==media.AssetId,ct),"Selected image does not exist.");
                var usage=new RevisionMedia{Revision=revision,MediaAssetId=media.AssetId,UsageKey=media.UsageKey,DisplayOrder=index};db.Add(usage);usages.Add(media.UsageKey,usage);
                db.Add(new RevisionMediaTranslation{Revision=revision,RevisionMedia=usage,Culture="en",AltText=media.AltEn??"",Caption=media.CaptionEn});
                db.Add(new RevisionMediaTranslation{Revision=revision,RevisionMedia=usage,Culture="ar",AltText=media.AltAr??"",Caption=media.CaptionAr});
            }
            if(m.Kind is ContentKind.Page or ContentKind.Project)foreach(var (culture,seo) in new[]{("en",m.SeoEnglish),("ar",m.SeoArabic)}){
                db.Add(new SeoMetadata{Revision=revision,Culture=culture,Title=seo.Title??"",Description=seo.Description??"",OpenGraphTitle=seo.OpenGraphTitle,OpenGraphDescription=seo.OpenGraphDescription,CanonicalPath=string.IsNullOrWhiteSpace(seo.CanonicalPath)?null:seo.CanonicalPath,NoIndex=seo.NoIndex,OpenGraphMediaUsage=seo.OpenGraphUsageKey is string key?usages.GetValueOrDefault(key):null});
            }
            switch(m.Kind) {
                case ContentKind.Page:
                    db.Add(new ContentPage{Revision=revision});
                    foreach(var (section,index) in m.Sections.OrderBy(x=>x.DisplayOrder).Select((x,i)=>(x,i)))db.Add(new PageSection{Revision=revision,SectionRevisionId=await Dependency(section.ContentItemId,ContentKind.Section,ct),DisplayOrder=index});break;
                case ContentKind.Section: db.Add(new ContentSection{Revision=revision,SectionKind=m.SectionKind,LayoutKey=m.LayoutKey,Background=m.Background,IsVisible=m.IsVisible,ParallaxIntensity=m.ParallaxIntensity,PrimaryButtonTarget=m.PrimaryButtonTarget,SecondaryButtonTarget=m.SecondaryButtonTarget});break;
                case ContentKind.Project:
                    db.Add(new ContentProject{Revision=revision,YouTubeVideoId=string.IsNullOrWhiteSpace(m.YouTubeVideoId)?null:m.YouTubeVideoId,IsFeatured=m.IsFeatured,DisplayOrder=m.DisplayOrder,CoverMediaUsage=usages.GetValueOrDefault("cover")});
                    foreach(var usage in usages.Values)db.Add(new ProjectMedia{Revision=revision,MediaUsage=usage,DisplayOrder=usage.DisplayOrder});
                    foreach(var category in m.Categories)db.Add(new ProjectCategoryAssignment{Revision=revision,CategoryRevisionId=await Dependency(category,ContentKind.ProjectCategory,ct)});break;
                case ContentKind.ProjectCategory: db.Add(new ProjectCategory{Revision=revision});break;
                case ContentKind.Service: db.Add(new ContentService{Revision=revision,IconKey=m.IconKey,DisplayOrder=m.DisplayOrder,IsVisible=m.IsVisible,MediaUsage=usages.GetValueOrDefault("cover")});break;
                case ContentKind.Navigation:
                    db.Add(new NavigationSettings{Revision=revision,Location=m.Key});var links=new List<NavigationItem>();
                    foreach(var (link,index) in m.Links.Select((x,i)=>(x,i))) {
                        var nav=new NavigationItem{Revision=revision,Href=link.Href,DisplayOrder=index,IsVisible=link.IsVisible,Parent=link.ParentIndex is int parent?links[parent]:null};links.Add(nav);db.Add(nav);
                        db.Add(new NavigationTranslation{Revision=revision,NavigationItem=nav,Culture="en",Label=link.LabelEn??""});db.Add(new NavigationTranslation{Revision=revision,NavigationItem=nav,Culture="ar",Label=link.LabelAr??""});
                    }break;
                case ContentKind.Contact:
                    db.Add(new ContactSettings{Revision=revision});
                    foreach(var (contact,index) in m.Contacts.Select((x,i)=>(x,i))) {
                        var detail=new ContactDetail{Revision=revision,Kind=contact.Kind,Value=contact.Value,DisplayOrder=index};db.Add(detail);
                        db.Add(new ContactTranslation{Revision=revision,ContactDetail=detail,Culture="en",Label=contact.LabelEn??"",LocalizedValue=contact.ValueEn});db.Add(new ContactTranslation{Revision=revision,ContactDetail=detail,Culture="ar",Label=contact.LabelAr??"",LocalizedValue=contact.ValueAr});
                    }
                    foreach(var hours in m.Hours)db.Add(new BusinessHours{Revision=revision,DayOfWeek=hours.DayOfWeek,IsClosed=hours.IsClosed,OpensAt=hours.IsClosed?null:hours.OpensAt,ClosesAt=hours.IsClosed?null:hours.ClosesAt});break;
                case ContentKind.SiteSettings:db.Add(new SiteSettings{Revision=revision,SiteName=m.Site.SiteName,DefaultCulture=m.Site.DefaultCulture,DefaultTheme=m.Site.DefaultTheme,PublicBaseUrl=string.IsNullOrWhiteSpace(m.Site.PublicBaseUrl)?null:m.Site.PublicBaseUrl.TrimEnd('/'),LogoMediaUsage=usages.GetValueOrDefault("logo"),FaviconMediaUsage=usages.GetValueOrDefault("favicon")});break;
                case ContentKind.ThemeSettings:
                    var theme=new ThemeSettings{Revision=revision};foreach(var property in typeof(ThemeEdit).GetProperties())typeof(ThemeSettings).GetProperty(property.Name)!.SetValue(theme,property.GetValue(m.Theme));db.Add(theme);break;
                case ContentKind.Footer:db.Add(new FooterSettings{Revision=revision,ShowCopyright=m.ShowCopyright});break;
            }
            if(m.Kind is ContentKind.Contact or ContentKind.Footer or ContentKind.SiteSettings)foreach(var (social,index) in m.Social.Select((x,i)=>(x,i)))db.Add(new SocialLink{Revision=revision,Platform=social.Platform,Url=social.Url,DisplayOrder=index});
            await db.SaveChangesAsync(ct);
            item.DraftRevisionId=revision.Id;
            if(item.State==PublicationState.Archived)item.State=PublicationState.Draft;
            db.AuditLogs.Add(new(){ActorUserId=actorId,ActorLabel=actorLabel,Action=m.Id==0?"CreateDraft":"SaveDraft",ContentItemId=item.Id,RevisionId=revision.Id});
            await db.SaveChangesAsync(ct);if(tx is not null)await tx.CommitAsync(ct);return item.Id;
        }catch(DbUpdateConcurrencyException){throw new ContentConflictException();}
    }
    public async Task TransitionAsync(long id,long version,string action,bool verified,Guid actorId,string actorLabel,CancellationToken ct=default)
    {
        actorLabel=actorLabel[..Math.Min(actorLabel.Length,160)];
        Require(action is "publish" or "unpublish" or "archive","Unknown publication action.");
        await using var tx=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;
        var item=await db.ContentItems.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new KeyNotFoundException();
        Require(Supported.Contains(item.Kind),"Unsupported content module.");if(item.Version!=version)throw new ContentConflictException();
        var routes=await db.Set<PublishedRoute>().Where(x=>x.ContentItemId==id).ToListAsync(ct);db.RemoveRange(routes);
        if(action=="publish") {
            Require(verified,"Confirm that both translations, links and media have been reviewed before publication.");
            Require(item.DraftRevisionId.HasValue,"Save a draft before publication.");
            var revisionId=item.DraftRevisionId!.Value;
            var translations=await Children<ContentTranslation>(revisionId,ct);ContentRules.RequirePublishable(item.Kind,translations);
            if(item.Kind==ContentKind.Navigation) {
                var nav=await Children<NavigationItem>(revisionId,ct);var text=await Children<NavigationTranslation>(revisionId,ct);
                foreach(var link in nav.Where(x=>x.IsVisible))Require(text.Any(x=>x.NavigationItemId==link.Id&&x.Culture=="en"&&!string.IsNullOrWhiteSpace(x.Label))&&text.Any(x=>x.NavigationItemId==link.Id&&x.Culture=="ar"&&!string.IsNullOrWhiteSpace(x.Label)),"Visible navigation requires both labels.");
            }
            if(item.Kind==ContentKind.Contact) {
                var details=await Children<ContactDetail>(revisionId,ct);var text=await Children<ContactTranslation>(revisionId,ct);
                foreach(var detail in details)Require(text.Any(x=>x.ContactDetailId==detail.Id&&x.Culture=="en"&&!string.IsNullOrWhiteSpace(x.Label))&&text.Any(x=>x.ContactDetailId==detail.Id&&x.Culture=="ar"&&!string.IsNullOrWhiteSpace(x.Label)),"Contact details require both labels.");
            }
            item.IsDemo=false;item.State=PublicationState.Published;item.PublishedRevisionId=revisionId;
            if(item.Kind is ContentKind.Page or ContentKind.Project)foreach(var t in translations)db.Add(new PublishedRoute{ContentItemId=id,RevisionId=revisionId,Culture=t.Culture,Scope=item.Kind==ContentKind.Page?"pages":"projects",Slug=t.Slug!});
        } else {item.State=action=="archive"?PublicationState.Archived:PublicationState.Draft;item.PublishedRevisionId=null;}
        db.AuditLogs.Add(new(){ActorUserId=actorId,ActorLabel=actorLabel,Action=action,ContentItemId=id,RevisionId=item.DraftRevisionId,DetailsJson=JsonSerializer.Serialize(new{verified})});
        try{await db.SaveChangesAsync(ct);if(tx is not null)await tx.CommitAsync(ct);}catch(DbUpdateConcurrencyException){throw new ContentConflictException();}
    }
}
