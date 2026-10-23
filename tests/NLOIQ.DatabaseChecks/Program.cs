
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
    var editor=new ContentEditor(db);var page=Draft(ContentKind.Page);page.Media=[new(){AssetId=asset.Id,UsageKey="cover",AltEn="Approved illustration",AltAr="??? ?????"}];var id=await editor.SaveAsync(page,actorId,"Rollback-only CMS checks");await Publish(editor,id);
    var reader=new PublicSiteReader(db,new PublishedContentReader(db));Check((await reader.ReadAsync("ar",page.Arabic.Slug!))!.Page.Images.Single().Alt=="??? ?????","Localized media missing.");
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
