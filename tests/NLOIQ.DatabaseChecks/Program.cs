
using System.Text.Json;
using NLOIQ.Application.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MySql.Data.MySqlClient;
using NLOIQ.Domain.Content;
using NLOIQ.Infrastructure.Data;
using NLOIQ.Application.Validation;
var connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
if (string.IsNullOrEmpty(connection))
{
    using var local = JsonDocument.Parse(File.ReadAllText("src/NLOIQ.Web/appsettings.Local.json"));
    connection = local.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString();
}
var settings = new MySqlConnectionStringBuilder(connection);
if (!settings.Database.Equals("WebSiteNLOIQDb", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Database checks only permit the dedicated WebSiteNLOIQDb development schema.");
var options = new DbContextOptionsBuilder<NloDbContext>().UseMySQL(connection!).Options;
int baselineUsers,baselineItems,baselineRevisions,baselineTranslations;
await using (var initial = new NloDbContext(options)) { baselineUsers = await initial.Users.CountAsync();baselineItems=await initial.ContentItems.CountAsync();baselineRevisions=await initial.ContentRevisions.CountAsync();baselineTranslations=await initial.ContentTranslations.CountAsync(); }
static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
async Task Scenario(string name, Func<NloDbContext, Task> action)
{
    await using var db = new NloDbContext(options);
    await using var transaction = await db.Database.BeginTransactionAsync();
    try { await action(db); Console.WriteLine("PASS: " + name); }
    finally { await transaction.RollbackAsync(); }
}
static async Task Rejected(Func<Task> action, string fragment)
{
    try { await action(); } catch (InvalidOperationException error) when (error.Message.Contains(fragment, StringComparison.OrdinalIgnoreCase)) { return; }
    throw new Exception("Expected application rejection: " + fragment);
}
static async Task SqlRejected(Func<Task> action, int code)
{
    try { await action(); }
    catch (MySqlException error) when (error.Number == code) { return; }
    catch (DbUpdateException error) when (error.InnerException is MySqlException sql && sql.Number == code) { return; }
    throw new Exception("Expected database constraint error " + code);
}
static ContentRevision Revision(ContentItem item, int number, string title, bool arabic = true)
{
    var revision = new ContentRevision { ContentItem = item, RevisionNumber = number, ChangeNote = "Rollback-only database verification" };
    revision.Translations.Add(new() { Revision = revision, Culture = "en", Title = title, Slug = item.Key });
    if (arabic) revision.Translations.Add(new() { Revision = revision, Culture = "ar", Title = "محتوى للتحقّق داخل معاملة ملغاة", Slug = item.Key });
    return revision;
}
async Task<(ContentItem Item, ContentRevision Revision)> Service(NloDbContext db, bool publish = false, bool arabic = true, bool demo = false)
{
    var item = new ContentItem { Kind = ContentKind.Service, Key = "dbcheck-" + Guid.NewGuid().ToString("N"), IsDemo = demo };
    var revision = Revision(item, 1, "Published original", arabic);
    db.Add(new ContentService { Revision = revision, IconKey = "network" });
    await db.SaveChangesAsync(); item.DraftRevisionId = revision.Id;
    if (publish) { item.PublishedRevisionId = revision.Id; item.State = PublicationState.Published; }
    await db.SaveChangesAsync(); return (item, revision);
}
await Scenario("existing inventory is consistent and public reader exposes no demo content", async db => {
    Check(await db.ContentItems.CountAsync() == baselineItems && baselineItems>=22, "Existing inventory changed.");
    Check(await db.ContentTranslations.CountAsync() == baselineTranslations, "Existing translations changed.");
    Check(!await db.ContentItems.AnyAsync(x => x.State == PublicationState.Published && (x.IsDemo || x.PublishedRevisionId == null)), "Published pointer/demo invariant invalid.");
    Check(await db.Users.CountAsync() == baselineUsers && await db.Roles.CountAsync() == 3, "Role definitions and existing administrator accounts must be preserved.");
    foreach(var demo in await db.ContentItems.Where(x=>x.IsDemo).ToListAsync()) Check(await new PublishedContentReader(db).FindAsync(demo.Kind,demo.Key,"en") is null,"Demo content leaked.");
    var arabic = await db.ContentTranslations.Where(x => x.Culture == "ar").Select(x => x.Title).ToListAsync();
    Check(arabic.Any(x => x.Contains("هندسة")) && arabic.All(x => !x.Contains("??")), "Arabic encoding damaged.");
    Check(await db.ContentRevisions.AllAsync(x => x.CreatedUtc < DateTime.UtcNow.AddMinutes(1)), "Invalid timestamp.");
    Check((await db.ContentRevisions.FirstAsync()).CreatedUtc.Kind == DateTimeKind.Utc, "UTC round trip lost.");
});
await Scenario("draft changes and stale routes never replace published content", async db => {
    var (item, first) = await Service(db, publish:true);
    var next = Revision(item, 2, "Unpublished draft change");
    db.Add(new ContentService { Revision = next, IconKey = "code" }); await db.SaveChangesAsync();
    item.DraftRevisionId = next.Id; await db.SaveChangesAsync();
    var reader = new PublishedContentReader(db);
    Check((await reader.FindAsync(item.Kind,item.Key,"en"))?.Title == "Published original", "Draft leaked to public reader.");
    var route = new PublishedRoute { Culture="en", Scope="services", Slug=item.Key, ContentItemId=item.Id, RevisionId=first.Id };
    db.Add(route); await db.SaveChangesAsync(); Check(await reader.FindRouteAsync("services",item.Key,"en") is not null,"Published route missing.");
    route.RevisionId=next.Id; await db.SaveChangesAsync(); Check(await reader.FindRouteAsync("services",item.Key,"en") is null,"Stale route exposed a draft.");
    item.State=PublicationState.Archived; await db.SaveChangesAsync(); Check(await reader.FindAsync(item.Kind,item.Key,"en") is null,"Archived content leaked.");
});
await Scenario("saved translations are immutable and late children are rejected", async db => {
    var (_,revision)=await Service(db);
    revision.Translations[0].Title="Attempted overwrite";
    await Rejected(()=>db.SaveChangesAsync(),"append-only");
    db.ChangeTracker.Clear();
    var saved=await db.ContentRevisions.SingleAsync(x=>x.Id==revision.Id);
    db.Add(new ContentTranslation { Revision=saved, Culture="en", Title="Late child" });
    await Rejected(()=>db.SaveChangesAsync(),"new revision");
});
await Scenario("missing translations and demo content cannot be published", async db => {
    var (item,revision)=await Service(db,arabic:false);
    item.State=PublicationState.Published;item.PublishedRevisionId=revision.Id;
    await Rejected(()=>db.SaveChangesAsync(),"English and Arabic");
});
await Scenario("demo content requires explicit verification before publication", async db => {
    var (item,revision)=await Service(db,demo:true);item.State=PublicationState.Published;item.PublishedRevisionId=revision.Id;
    await Rejected(()=>db.SaveChangesAsync(),"Demo content");
});
await Scenario("database enforces same-owner revision pointers", async db => {
    var (first,_)=await Service(db);var (_,foreign)=await Service(db);
    await SqlRejected(()=>db.Database.ExecuteSqlInterpolatedAsync($"UPDATE ContentItems SET DraftRevisionId={foreign.Id} WHERE Id={first.Id}"),1452);
});
await Scenario("database rejects duplicate translations and unsupported cultures", async db => {
    var (_,revision)=await Service(db);
    await SqlRejected(()=>db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO ContentTranslations (RevisionId,Culture,Title,Summary,Body) VALUES ({revision.Id},'en','Duplicate','','')"),1062);
    await SqlRejected(()=>db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO ContentTranslations (RevisionId,Culture,Title,Summary,Body) VALUES ({revision.Id},'fr','Unsupported','','')"),3819);
});
await Scenario("referenced media cannot be deleted", async db => {
    var asset=await db.MediaAssets.FirstAsync();
    await SqlRejected(()=>db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM MediaAssets WHERE Id={asset.Id}"),1451);
});
await Scenario("duplicate active localized routes are rejected", async db => {
    var (first,one)=await Service(db,publish:true);var (second,two)=await Service(db,publish:true);
    db.Add(new PublishedRoute{Culture="en",Scope="services",Slug=first.Key,ContentItemId=first.Id,RevisionId=one.Id});await db.SaveChangesAsync();
    await SqlRejected(()=>db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO PublishedRoutes (Culture,Scope,Slug,ContentItemId,RevisionId) VALUES ('en','services',{first.Key},{second.Id},{two.Id})"),1062);
});
await Scenario("optimistic concurrency rejects a stale writer", async db => {
    var (item,_)=await Service(db);
    var sharedOptions=new DbContextOptionsBuilder<NloDbContext>().UseMySQL(db.Database.GetDbConnection()).Options;
    await using var stale=new NloDbContext(sharedOptions);await stale.Database.UseTransactionAsync(db.Database.CurrentTransaction!.GetDbTransaction());
    var other=await stale.ContentItems.SingleAsync(x=>x.Id==item.Id);
    item.Key+="-first";await db.SaveChangesAsync();other.Key+="-stale";
    try {await stale.SaveChangesAsync();}catch(DbUpdateConcurrencyException){return;}
    throw new Exception("Stale write was accepted.");
});
await Scenario("page composition pins revisions and rejects draft section publication", async db => {
    var sectionItem=new ContentItem{Kind=ContentKind.Section,Key="dbcheck-"+Guid.NewGuid().ToString("N")};
    var section=new ContentSection{Revision=Revision(sectionItem,1,"Original section"),SectionKind=SectionKind.About};
    var pageItem=new ContentItem{Kind=ContentKind.Page,Key="dbcheck-"+Guid.NewGuid().ToString("N")};
    var page=new ContentPage{Revision=Revision(pageItem,1,"Page")};
    db.Add(section);db.Add(page);db.Add(new PageSection{Revision=page.Revision,Section=section,DisplayOrder=0});await db.SaveChangesAsync();
    var next=new ContentSection{Revision=Revision(sectionItem,2,"Changed section"),SectionKind=SectionKind.About};db.Add(next);await db.SaveChangesAsync();
    sectionItem.DraftRevisionId=next.RevisionId;await db.SaveChangesAsync();
    Check((await db.Set<PageSection>().SingleAsync(x=>x.RevisionId==page.RevisionId)).SectionRevisionId==section.RevisionId,"Page reference moved to a draft.");
    pageItem.PublishedRevisionId=page.RevisionId;pageItem.State=PublicationState.Published;
    await Rejected(()=>db.SaveChangesAsync(),"published, verified section");
});
await Scenario("audit history cannot be modified or deleted", async db => {
    var audit=new AuditLog{ActorLabel="Rollback test",Action="VerifyAppendOnly",DetailsJson="{}"};db.Add(audit);await db.SaveChangesAsync();
    audit.Action="Overwrite";await Rejected(()=>db.SaveChangesAsync(),"append-only");
});

await Scenario("long Arabic bodies use text storage without row-size failures", async db => {
    var item=new ContentItem{Kind=ContentKind.Service,Key="dbcheck-"+Guid.NewGuid().ToString("N")};
    var revision=Revision(item,1,new string('t',200));revision.Translations[1].Body=new string('م',16000);revision.Translations[1].Summary=new string('س',2000);
    db.Add(new ContentService{Revision=revision,IconKey="network"});await db.SaveChangesAsync();
    var body=await db.ContentTranslations.AsNoTracking().Where(x=>x.RevisionId==revision.Id&&x.Culture=="ar").Select(x=>x.Body).SingleAsync();
    Check(body.Length==16000&&body[0]=='م',"Long Arabic body did not round-trip.");
});
await Scenario("project cover media must belong to the same revision", async db => {
    var item=new ContentItem{Kind=ContentKind.Project,Key="dbcheck-"+Guid.NewGuid().ToString("N")};var revision=Revision(item,1,"Project");
    db.Add(new ContentProject{Revision=revision});await db.SaveChangesAsync();
    var foreignMedia=await db.Set<RevisionMedia>().FirstAsync();
    await SqlRejected(()=>db.Database.ExecuteSqlInterpolatedAsync($"UPDATE Projects SET CoverMediaUsageId={foreignMedia.Id} WHERE RevisionId={revision.Id}"),1452);
});
await Scenario("publication requires both media alt-text snapshots", async db => {
    var item=new ContentItem{Kind=ContentKind.Project,Key="dbcheck-"+Guid.NewGuid().ToString("N")};var revision=Revision(item,1,"Project");
    var asset=new MediaAsset{StorageProvider="Local",StorageKey="dbcheck/"+Guid.NewGuid().ToString("N")+".png",OriginalFileName="rollback-test.png",ContentType="image/png",ByteSize=1,Width=1,Height=1,Sha256=new string('0',64)};
    var media=new RevisionMedia{Revision=revision,MediaAsset=asset,UsageKey="cover"};
    db.Add(new ContentProject{Revision=revision,CoverMediaUsage=media});db.Add(new RevisionMediaTranslation{Revision=revision,RevisionMedia=media,Culture="en",AltText="Fixture"});await db.SaveChangesAsync();
    item.State=PublicationState.Published;item.PublishedRevisionId=revision.Id;
    await Rejected(()=>db.SaveChangesAsync(),"English and Arabic alt");
});
await Scenario("snapshot types must match their stable content kind", async db => {
    var item=new ContentItem{Kind=ContentKind.Service,Key="dbcheck-"+Guid.NewGuid().ToString("N")};
    db.Add(new ContentProject{Revision=Revision(item,1,"Wrong snapshot")});
    await Rejected(()=>db.SaveChangesAsync(),"Snapshot kind");
});


Guid actorId;
await using(var actorDb=new NloDbContext(options)) actorId=await actorDb.Users.Select(x=>x.Id).FirstAsync();
ContentEdit Draft(ContentKind kind,string? key=null)=>new(){Kind=kind,Key=key??"cmscheck-"+Guid.NewGuid().ToString("N"),English=new(){Title="CMS integration check",Slug="cmscheck-"+Guid.NewGuid().ToString("N")},Arabic=new(){Title="تحقق من إدارة المحتوى",Slug="cmscheck-"+Guid.NewGuid().ToString("N")}};
async Task Publish(ContentEditor editor,long id){var form=(await editor.GetAsync(id))!;await editor.TransitionAsync(id,form.Version,"publish",true,actorId,"Rollback-only CMS checks");}
await Scenario("CMS service CRUD preserves published content until explicit publication",async db=>{
    var editor=new ContentEditor(db);var form=Draft(ContentKind.Service);var id=await editor.SaveAsync(form,actorId,"Rollback-only CMS checks");
    Check((await editor.GetAsync(id))!.English.Title==form.English.Title,"Draft read failed.");await Publish(editor,id);
    var reader=new PublishedContentReader(db);var first=await reader.FindAsync(ContentKind.Service,form.Key,"en");
    var update=(await editor.GetAsync(id))!;update.English.Title="Updated service title";await editor.SaveAsync(update,actorId,"Rollback-only CMS checks");
    Check((await reader.FindAsync(ContentKind.Service,form.Key,"en"))!.Title==first!.Title,"Unpublished edit leaked.");
    await Publish(editor,id);Check((await reader.FindAsync(ContentKind.Service,form.Key,"en"))!.Title=="Updated service title","Publication failed.");
    var current=(await editor.GetAsync(id))!;await editor.TransitionAsync(id,current.Version,"archive",false,actorId,"Rollback-only CMS checks");Check(await reader.FindAsync(ContentKind.Service,form.Key,"en") is null,"Archive leaked.");
    await editor.SaveAsync((await editor.GetAsync(id))!,actorId,"Rollback-only CMS checks");Check((await db.ContentItems.SingleAsync(x=>x.Id==id)).State==PublicationState.Draft,"Archive restore failed.");
});
await Scenario("CMS stale forms cannot create a revision or overwrite newer drafts",async db=>{
    var editor=new ContentEditor(db);var id=await editor.SaveAsync(Draft(ContentKind.Service),actorId,"Rollback-only CMS checks");var stale=(await editor.GetAsync(id))!;await editor.SaveAsync((await editor.GetAsync(id))!,actorId,"Rollback-only CMS checks");
    var before=await db.ContentRevisions.CountAsync();try{await editor.SaveAsync(stale,actorId,"Rollback-only CMS checks");throw new Exception("Stale form accepted.");}catch(ContentConflictException){}
    Check(await db.ContentRevisions.CountAsync()==before,"Conflict left a revision.");
});
await Scenario("CMS sections and page composition pin published revisions and republish explicitly",async db=>{
    var editor=new ContentEditor(db);var section=Draft(ContentKind.Section);section.SectionKind=SectionKind.Hero;var sid=await editor.SaveAsync(section,actorId,"Rollback-only CMS checks");await Publish(editor,sid);
    var page=Draft(ContentKind.Page);page.Sections=[new(){ContentItemId=sid}];var pid=await editor.SaveAsync(page,actorId,"Rollback-only CMS checks");await Publish(editor,pid);
    var reader=new PublicSiteReader(db,new PublishedContentReader(db));var slug=(await editor.GetAsync(pid))!.English.Slug!;
    var old=(await reader.ReadAsync("en",slug))!.Sections[0].Title;
    var update=(await editor.GetAsync(sid))!;update.English.Title="New hero revision";await editor.SaveAsync(update,actorId,"Rollback-only CMS checks");await Publish(editor,sid);
    Check((await reader.ReadAsync("en",slug))!.Sections[0].Title==old,"Page composition unexpectedly changed.");
    await editor.SaveAsync((await editor.GetAsync(pid))!,actorId,"Rollback-only CMS checks");await Publish(editor,pid);Check((await reader.ReadAsync("en",slug))!.Sections[0].Title=="New hero revision","Page did not refresh dependency.");
});
await Scenario("CMS project save clones gallery alt text, categories and SEO without changing old snapshots",async db=>{
    var editor=new ContentEditor(db);var root=await db.ContentItems.SingleAsync(x=>x.Kind==ContentKind.Project&&x.Key=="network-study");var previous=root.DraftRevisionId;
    var form=(await editor.GetAsync(root.Id))!;await editor.SaveAsync(form,actorId,"Rollback-only CMS checks");var next=(await editor.GetAsync(root.Id))!;
    Check(next.Media.Count==form.Media.Count&&next.Categories.SequenceEqual(form.Categories),"Project composition lost.");Check(await db.Set<SeoMetadata>().CountAsync(x=>x.RevisionId==root.DraftRevisionId)==2,"SEO snapshots lost.");
    Check(await db.Set<RevisionMedia>().CountAsync(x=>x.RevisionId==previous)==1,"Old media snapshot changed.");await Rejected(()=>Publish(editor,root.Id),"Demo media");
});
await Scenario("CMS navigation contact and footer structured fields round-trip",async db=>{
    var editor=new ContentEditor(db);
    foreach(var (kind,key) in new[]{(ContentKind.Navigation,"main"),(ContentKind.Contact,"primary"),(ContentKind.Footer,"main")}){
        var root=await db.ContentItems.SingleAsync(x=>x.Kind==kind&&x.Key==key);var form=(await editor.GetAsync(root.Id))!;
        if(kind==ContentKind.Navigation)form.Links=[new(){LabelEn="Parent",LabelAr="رئيسي",Href="#main"},new(){LabelEn="Child",LabelAr="فرعي",Href="/en/pages/cms-review",ParentIndex=0}];
        if(kind==ContentKind.Contact){form.Contacts=[new(){Kind=ContactKind.Email,Value="test@example.invalid",LabelEn="Test email",LabelAr="بريد الاختبار"}];form.Hours=[new(){DayOfWeek=1,OpensAt=new(9,0),ClosesAt=new(17,0)}];}
        if(kind==ContentKind.Footer)form.Social=[new(){Platform="linkedin",Url="https://example.invalid/test"}];
        await editor.SaveAsync(form,actorId,"Rollback-only CMS checks");await Publish(editor,root.Id);var saved=(await editor.GetAsync(root.Id))!;
        Check(saved.Links.Count==form.Links.Count&&saved.Contacts.Count==form.Contacts.Count&&saved.Social.Count==form.Social.Count&&saved.Hours.Count==form.Hours.Count,"Structured fields lost.");
    }
});
await Scenario("CMS unsafe links and invalid relationships are rejected before writing",async db=>{
    var editor=new ContentEditor(db);var form=Draft(ContentKind.Section);form.PrimaryButtonTarget="javascript:alert(1)";await Rejected(()=>editor.SaveAsync(form,actorId,"Rollback-only CMS checks"),"Unsafe section link");
});
await Scenario("CMS missing translations and unverified publication are rejected",async db=>{
    var editor=new ContentEditor(db);var form=Draft(ContentKind.Page);form.Arabic.Title="";var id=await editor.SaveAsync(form,actorId,"Rollback-only CMS checks");
    var current=(await editor.GetAsync(id))!;await Rejected(()=>editor.TransitionAsync(id,current.Version,"publish",false,actorId,"Rollback-only CMS checks"),"Confirm");await Rejected(()=>Publish(editor,id),"Both English");
});
await Scenario("CMS changes produce append-only audit records with actor and revision",async db=>{
    var editor=new ContentEditor(db);var id=await editor.SaveAsync(Draft(ContentKind.ProjectCategory),actorId,"Rollback-only CMS checks");await Publish(editor,id);
    var audit=await db.AuditLogs.Where(x=>x.ContentItemId==id).ToListAsync();Check(audit.Count==2&&audit.All(x=>x.ActorUserId==actorId&&x.RevisionId!=null),"Audit information missing.");
});
await Scenario("CMS public composition filters hidden services and unpublished projects",async db=>{
    var editor=new ContentEditor(db);var page=Draft(ContentKind.Page);var pid=await editor.SaveAsync(page,actorId,"Rollback-only CMS checks");await Publish(editor,pid);
    var service=Draft(ContentKind.Service);service.IsVisible=false;var sid=await editor.SaveAsync(service,actorId,"Rollback-only CMS checks");await Publish(editor,sid);
    var category=Draft(ContentKind.ProjectCategory);var cid=await editor.SaveAsync(category,actorId,"Rollback-only CMS checks");await Publish(editor,cid);
    var project=Draft(ContentKind.Project);project.Categories=[cid];project.YouTubeVideoId="NNiszX0-A7M";var projectId=await editor.SaveAsync(project,actorId,"Rollback-only CMS checks");
    var reader=new PublicSiteReader(db,new PublishedContentReader(db));var live=(await reader.ReadAsync("en",page.English.Slug!))!;
    Check(live.Services.All(x=>x.Id!=sid)&&live.Projects.All(x=>x.Id!=projectId),"Hidden/unpublished content leaked.");
    await Publish(editor,projectId);var update=(await editor.GetAsync(sid))!;update.IsVisible=true;await editor.SaveAsync(update,actorId,"Rollback-only CMS checks");await Publish(editor,sid);
    live=(await reader.ReadAsync("en",page.English.Slug!))!;Check(live.Services.Any(x=>x.Id==sid)&&live.Projects.Any(x=>x.Id==projectId&&x.VideoId=="NNiszX0-A7M"&&x.Categories.Count==1),"Published project/service missing.");
});
await Scenario("CMS approved image selection reaches published public media without exposing demo assets",async db=>{
    var source=await db.MediaAssets.FirstAsync();var asset=new MediaAsset{StorageProvider="Bundled",StorageKey="images/cmscheck-approved.svg",OriginalFileName="cmscheck-approved.svg",ContentType="image/svg+xml",ByteSize=source.ByteSize,Width=source.Width,Height=source.Height,Sha256=source.Sha256,IsDemo=false};db.Add(asset);await db.SaveChangesAsync();
    var editor=new ContentEditor(db);var page=Draft(ContentKind.Page);page.Media=[new(){AssetId=asset.Id,UsageKey="cover",AltEn="Approved illustration",AltAr="رسم معتمد"}];var id=await editor.SaveAsync(page,actorId,"Rollback-only CMS checks");await Publish(editor,id);
    var reader=new PublicSiteReader(db,new PublishedContentReader(db));Check((await reader.ReadAsync("ar",page.Arabic.Slug!))!.Page.Images.Single().Alt=="رسم معتمد","Localized media missing.");
});
await Scenario("SEO draft metadata is isolated and published metadata persists in both languages",async db=>{
    var editor=new ContentEditor(db);var form=Draft(ContentKind.Page);form.SeoEnglish=new(){Title="Search title",Description="Search description",CanonicalPath="/en/pages/review",NoIndex=false};form.SeoArabic=new(){Title="عنوان البحث",Description="وصف البحث"};
    var id=await editor.SaveAsync(form,actorId,"Rollback-only SEO check");await Publish(editor,id);var reader=new PublicSiteReader(db,new PublishedContentReader(db));
    Check((await reader.ReadAsync("en",form.English.Slug!))!.Page.Seo!.Title=="Search title","Published SEO missing.");
    var update=(await editor.GetAsync(id))!;update.SeoEnglish.Title="Private SEO draft";await editor.SaveAsync(update,actorId,"Rollback-only SEO check");Check((await reader.ReadAsync("en",form.English.Slug!))!.Page.Seo!.Title=="Search title","Draft SEO leaked.");await Publish(editor,id);Check((await reader.ReadAsync("en",form.English.Slug!))!.Page.Seo!.Title=="Private SEO draft","SEO publish failed.");Check((await reader.ReadAsync("ar",form.Arabic.Slug!))!.Page.Seo!.Title=="عنوان البحث","Arabic SEO lost.");
});
await Scenario("site settings and palette drafts apply only after publication",async db=>{
    var editor=new ContentEditor(db);var brand=new BrandReader(db);var before=await brand.ReadAsync("en");
    var root=await db.ContentItems.SingleAsync(x=>x.Kind==ContentKind.SiteSettings&&x.Key=="site");var form=(await editor.GetAsync(root.Id))!;form.Site.SiteName="NLO IQ review";form.Site.DefaultCulture="ar";form.Site.DefaultTheme="dark";form.Social=[new(){Platform="linkedin",Url="https://example.invalid/profile"}];await editor.SaveAsync(form,actorId,"Rollback-only settings check");Check((await brand.ReadAsync("en")).SiteName==before.SiteName,"Site draft leaked.");await Publish(editor,root.Id);var saved=await brand.ReadAsync("ar");Check(saved.SiteName=="NLO IQ review"&&saved.DefaultCulture=="ar"&&saved.DefaultTheme=="dark"&&saved.Social.Count==1,"Site defaults lost.");
    root=await db.ContentItems.SingleAsync(x=>x.Kind==ContentKind.ThemeSettings&&x.Key=="brand");form=(await editor.GetAsync(root.Id))!;form.Theme.Copper="#A05A40";await editor.SaveAsync(form,actorId,"Rollback-only palette check");var revision=(await db.ContentItems.SingleAsync(x=>x.Id==root.Id)).DraftRevisionId!.Value;Check(await brand.ThemeCssAsync(revision) is null,"Draft CSS leaked.");await Publish(editor,root.Id);Check((await brand.ThemeCssAsync(revision))!.Contains("--copper:#A05A40"),"Published palette missing.");
});
await Scenario("invalid canonical paths and low-contrast palettes are rejected",async db=>{
    var editor=new ContentEditor(db);var form=Draft(ContentKind.Page);form.SeoEnglish.CanonicalPath="//evil.example/path";await Rejected(()=>editor.SaveAsync(form,actorId,"Rollback-only SEO check"),"Canonical");
});
await Scenario("theme contrast guard rejects unreadable custom tokens",async db=>{
    var editor=new ContentEditor(db);var root=await db.ContentItems.SingleAsync(x=>x.Kind==ContentKind.ThemeSettings);var form=(await editor.GetAsync(root.Id))!;form.Theme.Ivory=form.Theme.Evergreen;await Rejected(()=>editor.SaveAsync(form,actorId,"Rollback-only palette check"),"contrast");
});
await Scenario("draft image selections do not make new assets publicly readable",async db=>{
    var editor=new ContentEditor(db);var page=Draft(ContentKind.Page);var id=await editor.SaveAsync(page,actorId,"Rollback-only media access check");await Publish(editor,id);
    var asset=new MediaAsset{StorageProvider="Local",StorageKey=Guid.NewGuid().ToString("N")+"/main.webp",OriginalFileName="private.webp",ContentType="image/webp",ByteSize=10,Width=10,Height=10,Sha256=new string('a',64)};db.Add(asset);await db.SaveChangesAsync();
    var update=(await editor.GetAsync(id))!;update.Media=[new(){AssetId=asset.Id,UsageKey="cover",AltEn="Private draft image",AltAr="صورة مسودة خاصة"}];await editor.SaveAsync(update,actorId,"Rollback-only media access check");Check(!await NLOIQ.Infrastructure.Media.MediaAccess.IsPublicAsync(db,asset.Id),"Draft image leaked.");await Publish(editor,id);Check(await NLOIQ.Infrastructure.Media.MediaAccess.IsPublicAsync(db,asset.Id),"Published image blocked.");
    update=(await editor.GetAsync(id))!;await editor.TransitionAsync(id,update.Version,"unpublish",false,actorId,"Rollback-only media access check");Check(!await NLOIQ.Infrastructure.Media.MediaAccess.IsPublicAsync(db,asset.Id),"Unpublished image remained public.");
});
await Scenario("independent Arabic slugs and localized editor titles survive publication",async db=>{
    var editor=new ContentEditor(db);var form=Draft(ContentKind.Page);form.Arabic.Slug="مراجعة-لغات-"+Guid.NewGuid().ToString("N");
    var id=await editor.SaveAsync(form,actorId,"Rollback-only localization check");await Publish(editor,id);
    var reader=new PublicSiteReader(db,new PublishedContentReader(db));var ar=(await reader.ReadAsync("ar",form.Arabic.Slug))!;var en=(await reader.ReadAsync("en",form.English.Slug!))!;
    Check(ar.Page.Title==form.Arabic.Title&&en.Page.Title==form.English.Title,"Localized public title missing.");
    Check(ar.AlternateUrl=="/en/pages/"+form.English.Slug&&en.AlternateUrl=="/ar/pages/"+form.Arabic.Slug,"Alternate uses the wrong language slug.");
    Check((await editor.ListAsync(ContentKind.Page,form.Key,default,"ar")).Single().Title==form.Arabic.Title,"Arabic listing uses English title.");
    var update=(await editor.GetAsync(id))!;update.Arabic.Title="عنوان مسودة خاص";await editor.SaveAsync(update,actorId,"Rollback-only localization check");
    Check((await reader.ReadAsync("ar",form.Arabic.Slug))!.Page.Title==form.Arabic.Title,"Private Arabic draft leaked.");
    Check(await reader.ReadAsync("en",form.Arabic.Slug) is null,"Arabic route resolved under English prefix.");
});
await Scenario("private revision previews include drafts without changing public eligibility",async db=>{
    var editor=new ContentEditor(db);var form=Draft(ContentKind.Page);var id=await editor.SaveAsync(form,actorId,"Rollback-only preview check");var revision=(await db.ContentItems.SingleAsync(x=>x.Id==id)).DraftRevisionId!.Value;
    var reader=new PublicSiteReader(db,new PublishedContentReader(db));Check(await reader.ReadAsync("en",form.English.Slug) is null,"Draft route leaked.");
    Check((await reader.PreviewAsync(id,revision,"ar")) is {IsPreview:true} preview&&preview.Page.Title==form.Arabic.Title,"Private Arabic preview missing.");
    var other=await editor.SaveAsync(Draft(ContentKind.Page),actorId,"Rollback-only preview check");Check(await reader.PreviewAsync(other,revision,"en") is null,"Cross-owner revision preview allowed.");
});
await Scenario("restore creates an immutable draft and preserves exact pinned section revisions",async db=>{
    var editor=new ContentEditor(db);var workflow=new ContentWorkflow(db,editor,TimeProvider.System);
    var block=Draft(ContentKind.Section);var sid=await editor.SaveAsync(block,actorId,"Rollback-only restore check");await Publish(editor,sid);
    var page=Draft(ContentKind.Page);page.Sections=[new(){ContentItemId=sid}];var id=await editor.SaveAsync(page,actorId,"Rollback-only restore check");await Publish(editor,id);var root=await db.ContentItems.SingleAsync(x=>x.Id==id);var source=root.DraftRevisionId!.Value;var pin=await db.Set<PageSection>().Where(x=>x.RevisionId==source).Select(x=>x.SectionRevisionId).SingleAsync();
    var update=(await editor.GetAsync(sid))!;update.English.Title="Updated linked section";await editor.SaveAsync(update,actorId,"Rollback-only restore check");await Publish(editor,sid);
    var changed=(await editor.GetAsync(id))!;changed.English.Title="Private replacement";await editor.SaveAsync(changed,actorId,"Rollback-only restore check");var before=root.Version;
    await workflow.RestoreAsync(id,source,before,actorId,"Rollback-only restore check");Check(root.DraftRevisionId!=source&&root.PublishedRevisionId==source,"Restore changed history or publication.");
    Check((await editor.GetAsync(id))!.English.Title==page.English.Title,"Restored text differs.");Check(await db.Set<PageSection>().Where(x=>x.RevisionId==root.DraftRevisionId).Select(x=>x.SectionRevisionId).SingleAsync()==pin,"Restore silently retargeted a section.");
    Check((await workflow.HistoryAsync(id))!.Revisions.Count==3,"History missing revisions.");
    try{await workflow.RestoreAsync(id,source,before,actorId,"stale restore");throw new Exception("Stale restore accepted");}catch(ContentConflictException){}
    var foreign=await db.ContentRevisions.Where(x=>x.ContentItemId==sid).Select(x=>x.Id).FirstAsync();try{await workflow.RestoreAsync(id,foreign,root.Version,actorId,"foreign restore");throw new Exception("Cross-owner restore accepted");}catch(KeyNotFoundException){}
});
await Scenario("restore remaps project cover gallery alt and SEO relationships",async db=>{
    var root=new ContentItem{Kind=ContentKind.Project,Key="restore-"+Guid.NewGuid().ToString("N")};var source=Revision(root,1,"Original project");var asset=await db.MediaAssets.FirstAsync();
    var media=new RevisionMedia{Revision=source,MediaAssetId=asset.Id,UsageKey="hero",DisplayOrder=4};db.Add(new ContentProject{Revision=source,CoverMediaUsage=media,DisplayOrder=3,YouTubeVideoId="NNiszX0-A7M"});db.Add(new ProjectMedia{Revision=source,MediaUsage=media,DisplayOrder=7});
    db.Add(new RevisionMediaTranslation{Revision=source,RevisionMedia=media,Culture="en",AltText="Historical image",Caption="Historical caption"});db.Add(new RevisionMediaTranslation{Revision=source,RevisionMedia=media,Culture="ar",AltText="صورة تاريخية"});
    db.Add(new SeoMetadata{Revision=source,Culture="en",Title="Historical SEO",OpenGraphMediaUsage=media});await db.SaveChangesAsync();root.DraftRevisionId=source.Id;await db.SaveChangesAsync();
    var editor=new ContentEditor(db);var workflow=new ContentWorkflow(db,editor,TimeProvider.System);await workflow.RestoreAsync(root.Id,source.Id,root.Version,actorId,"Rollback-only graph check");var revision=root.DraftRevisionId!.Value;
    var copy=await db.Set<ContentProject>().SingleAsync(x=>x.RevisionId==revision);var copiedMedia=await db.Set<RevisionMedia>().SingleAsync(x=>x.RevisionId==revision);Check(copy.CoverMediaUsageId==copiedMedia.Id&&copiedMedia.Id!=media.Id&&copiedMedia.DisplayOrder==4,"Cover remapping failed.");
    Check((await db.Set<ProjectMedia>().SingleAsync(x=>x.RevisionId==revision)).MediaUsageId==copiedMedia.Id,"Gallery remapping failed.");Check((await db.Set<ProjectMedia>().SingleAsync(x=>x.RevisionId==revision)).DisplayOrder==7,"Gallery ordering changed.");Check((await db.Set<SeoMetadata>().SingleAsync(x=>x.RevisionId==revision)).OpenGraphMediaUsageId==copiedMedia.Id,"SEO image remapping failed.");Check((await db.Set<RevisionMediaTranslation>().SingleAsync(x=>x.RevisionId==revision&&x.Culture=="en")).Caption=="Historical caption","Historical caption lost.");
});
await Scenario("restore preserves navigation parent and target references and all settings payloads",async db=>{
    var editor=new ContentEditor(db);var workflow=new ContentWorkflow(db,editor,TimeProvider.System);
    foreach(var kind in new[]{ContentKind.Navigation,ContentKind.Contact,ContentKind.Footer,ContentKind.SiteSettings,ContentKind.ThemeSettings,ContentKind.Service,ContentKind.ProjectCategory,ContentKind.Section}) {
        var root=await db.ContentItems.FirstAsync(x=>x.Kind==kind);
        if(kind==ContentKind.Contact){var form=(await editor.GetAsync(root.Id))!;form.Contacts=[new(){Kind=ContactKind.Email,Value="review@example.invalid",LabelEn="Review email",LabelAr="بريد المراجعة"}];form.Hours=[new(){DayOfWeek=0,OpensAt=new TimeOnly(8,30),ClosesAt=new TimeOnly(17,15)}];form.Social=[new(){Platform="linkedin",Url="https://example.invalid/review"}];await editor.SaveAsync(form,actorId,"Rollback-only structured restore check");}
        if(kind==ContentKind.SiteSettings){var form=(await editor.GetAsync(root.Id))!;var asset=await db.MediaAssets.FirstAsync();form.Site.LogoAssetId=asset.Id;form.Site.FaviconAssetId=asset.Id;form.Site.LogoAltEn="Review logo";form.Site.LogoAltAr="شعار للمراجعة";await editor.SaveAsync(form,actorId,"Rollback-only logo restore check");}
        var source=root.DraftRevisionId!.Value;var snapshot=(await editor.GetRevisionAsync(root.Id,source))!;
        await workflow.RestoreAsync(root.Id,source,root.Version,actorId,"Rollback-only module restore check");var copy=(await editor.GetAsync(root.Id))!;
        Check(copy.English.Title==snapshot.English.Title&&copy.Arabic.Body==snapshot.Arabic.Body,"Restored translation differs.");Check(copy.Links.Count==snapshot.Links.Count&&copy.Contacts.Count==snapshot.Contacts.Count&&copy.Hours.Count==snapshot.Hours.Count&&copy.Social.Count==snapshot.Social.Count,"Structured rows lost.");Check(copy.Site.SiteName==snapshot.Site.SiteName&&copy.Theme.Copper==snapshot.Theme.Copper&&copy.Site.LogoAssetId==snapshot.Site.LogoAssetId&&copy.Site.FaviconAssetId==snapshot.Site.FaviconAssetId,"Settings lost.");if(kind==ContentKind.Contact)Check(copy.Hours.Single().OpensAt==new TimeOnly(8,30)&&copy.Contacts.Single().Value=="review@example.invalid","Contact hours/value lost.");
    }
});
await Scenario("restored navigation remaps parent IDs while retaining target identities",async db=>{
    var root=new ContentItem{Kind=ContentKind.Navigation,Key="nav-restore-"+Guid.NewGuid().ToString("N")};var source=Revision(root,1,"Navigation restore check");db.Add(new NavigationSettings{Revision=source,Location="main"});var target=await db.ContentItems.Where(x=>x.Kind==ContentKind.Page).Select(x=>x.Id).FirstAsync();
    var parent=new NavigationItem{Revision=source,Href="/{culture}/",DisplayOrder=0};var child=new NavigationItem{Revision=source,Parent=parent,Href="#contact",TargetContentItemId=target,DisplayOrder=1};db.Add(parent);db.Add(child);
    foreach(var item in new[]{parent,child})foreach(var culture in new[]{"en","ar"})db.Add(new NavigationTranslation{Revision=source,NavigationItem=item,Culture=culture,Label=culture=="en"?"Review link":"رابط مراجعة"});await db.SaveChangesAsync();root.DraftRevisionId=source.Id;await db.SaveChangesAsync();
    var editor=new ContentEditor(db);var workflow=new ContentWorkflow(db,editor,TimeProvider.System);await workflow.RestoreAsync(root.Id,source.Id,root.Version,actorId,"Rollback-only hierarchy restore check");
    var copies=await db.Set<NavigationItem>().Where(x=>x.RevisionId==root.DraftRevisionId).OrderBy(x=>x.DisplayOrder).ToListAsync();Check(copies[0].Id!=parent.Id&&copies[1].ParentItemId==copies[0].Id&&copies[1].TargetContentItemId==target,"Hierarchy or stable target lost.");Check(await db.Set<NavigationTranslation>().CountAsync(x=>x.RevisionId==root.DraftRevisionId)==4,"Labels missing.");
});
await Scenario("scheduled publication pins a revision and executes once at due time",async db=>{
    var editor=new ContentEditor(db);var clock=new PublishingClock(DateTimeOffset.UtcNow);var workflow=new ContentWorkflow(db,editor,clock);var form=Draft(ContentKind.Page);var id=await editor.SaveAsync(form,actorId,"Rollback-only schedule check");var root=await db.ContentItems.SingleAsync(x=>x.Id==id);
    await workflow.ScheduleAsync(id,root.DraftRevisionId!.Value,root.Version,clock.GetUtcNow().UtcDateTime.AddMinutes(2),true,actorId,"Rollback-only schedule check");var job=await db.Set<PublicationSchedule>().SingleAsync(x=>x.ContentItemId==id);
    Check(root.State==PublicationState.Draft&&root.PublishedRevisionId is null,"Schedule published early.");await workflow.ExecuteAsync(job.Id);Check(job.State==ScheduleState.Pending,"Not-due job ran.");clock.Current=clock.Current.AddMinutes(3);Check((await workflow.DueAsync()).Contains(job.Id),"Due job missing.");await workflow.ExecuteAsync(job.Id);Check(job.State==ScheduleState.Completed&&root.PublishedRevisionId==job.RevisionId,"Due job did not publish pinned revision.");var count=await db.AuditLogs.CountAsync(x=>x.ContentItemId==id&&x.Action=="ScheduledPublish");await workflow.ExecuteAsync(job.Id);Check(await db.AuditLogs.CountAsync(x=>x.ContentItemId==id&&x.Action=="ScheduledPublish")==count,"Job ran twice.");
});
await Scenario("saving another draft and manual publication cancel pending schedules",async db=>{
    var editor=new ContentEditor(db);var clock=new PublishingClock(DateTimeOffset.UtcNow);var workflow=new ContentWorkflow(db,editor,clock);var id=await editor.SaveAsync(Draft(ContentKind.Page),actorId,"Rollback-only schedule check");var root=await db.ContentItems.SingleAsync(x=>x.Id==id);
    await workflow.ScheduleAsync(id,root.DraftRevisionId!.Value,root.Version,clock.GetUtcNow().UtcDateTime.AddMinutes(2),true,actorId,"Rollback-only schedule check");var update=(await editor.GetAsync(id))!;update.English.Title="Changed after approval";await editor.SaveAsync(update,actorId,"Rollback-only schedule check");Check((await db.Set<PublicationSchedule>().SingleAsync(x=>x.ContentItemId==id)).State==ScheduleState.Cancelled,"Draft save retained schedule.");
    await workflow.ScheduleAsync(id,root.DraftRevisionId!.Value,root.Version,clock.GetUtcNow().UtcDateTime.AddMinutes(2),true,actorId,"Rollback-only schedule check");await Publish(editor,id);Check(!await db.Set<PublicationSchedule>().AnyAsync(x=>x.ContentItemId==id&&x.State==ScheduleState.Pending),"Manual publish retained schedule.");
});
await Scenario("schedule cancellation rescheduling and publisher revocation are enforced",async db=>{
    var editor=new ContentEditor(db);var clock=new PublishingClock(DateTimeOffset.UtcNow);var workflow=new ContentWorkflow(db,editor,clock);var id=await editor.SaveAsync(Draft(ContentKind.Page),actorId,"Rollback-only schedule check");var root=await db.ContentItems.SingleAsync(x=>x.Id==id);var due=clock.GetUtcNow().UtcDateTime.AddMinutes(2);
    await workflow.ScheduleAsync(id,root.DraftRevisionId!.Value,root.Version,due,true,actorId,"Rollback-only schedule check");var job=await db.Set<PublicationSchedule>().SingleAsync(x=>x.ContentItemId==id);await workflow.CancelAsync(id,job.Id,root.Version,job.Version,actorId,"Rollback-only schedule check");
    await workflow.ScheduleAsync(id,root.DraftRevisionId!.Value,root.Version,due,true,actorId,"Rollback-only schedule check");var second=await db.Set<PublicationSchedule>().SingleAsync(x=>x.ContentItemId==id&&x.State==ScheduleState.Pending);var user=await db.Users.SingleAsync(x=>x.Id==actorId);user.IsActive=false;await db.SaveChangesAsync();clock.Current=clock.Current.AddMinutes(3);await workflow.ExecuteAsync(second.Id);Check(second.State==ScheduleState.Failed&&second.FailureCode=="PublisherUnavailable"&&root.PublishedRevisionId is null,"Revoked publisher's schedule executed.");
});
await Scenario("schedule confirmation time bounds and publication checks reject invalid jobs",async db=>{
    var editor=new ContentEditor(db);var clock=new PublishingClock(DateTimeOffset.UtcNow);var workflow=new ContentWorkflow(db,editor,clock);var form=Draft(ContentKind.Page);form.Arabic.Title="";var id=await editor.SaveAsync(form,actorId,"Rollback-only schedule check");var root=await db.ContentItems.SingleAsync(x=>x.Id==id);var due=clock.GetUtcNow().UtcDateTime.AddMinutes(2);
    await Rejected(()=>workflow.ScheduleAsync(id,root.DraftRevisionId!.Value,root.Version,due,false,actorId,"check"),"Confirm");await Rejected(()=>workflow.ScheduleAsync(id,root.DraftRevisionId!.Value,root.Version,clock.GetUtcNow().UtcDateTime,false,actorId,"check"),"Confirm");await Rejected(()=>workflow.ScheduleAsync(id,root.DraftRevisionId!.Value,root.Version,clock.GetUtcNow().UtcDateTime,true,actorId,"check"),"one minute");await Rejected(()=>workflow.ScheduleAsync(id,root.DraftRevisionId!.Value,root.Version,due,true,actorId,"check"),"titles");Check(!await db.Set<PublicationSchedule>().AnyAsync(x=>x.ContentItemId==id),"Invalid schedule persisted.");
});
var markup=ContentMarkup.Render("**bold** *italic* [safe](https://example.com) [bad](javascript:alert) <script>alert(1)</script>\n- Item");
Check(markup.Contains("<strong>bold</strong>")&&markup.Contains("<em>italic</em>")&&markup.Contains("<ul>")&&!markup.Contains("<script>")&&!markup.Contains("href=\"javascript:"),"Unsafe formatted text.");Console.WriteLine("PASS: formatted text is encoded and only safe links render");

await using(var verify=new NloDbContext(options))
{
    Check(await verify.Users.CountAsync()==baselineUsers,"Existing accounts changed.");
    Check(await verify.ContentItems.CountAsync()==baselineItems,"Test data was not rolled back.");
    Check(await verify.ServicePreviews.CountAsync()==4,"Preview data changed.");
    Check(await verify.ContentRevisions.CountAsync()==baselineRevisions,"Revision fixture count changed.");
    Console.WriteLine("PASS: all verification transactions rolled back; preview and draft fixtures retained.");
}

sealed class PublishingClock(DateTimeOffset now):TimeProvider { public DateTimeOffset Current {get;set;}=now; public override DateTimeOffset GetUtcNow()=>Current; }
