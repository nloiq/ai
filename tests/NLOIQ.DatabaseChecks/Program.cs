
using System.Text.Json;
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
await Scenario("seed is draft-only and public reader exposes no sample content", async db => {
    Check(await db.ContentItems.CountAsync() == 22, "Expected 22 initial draft items.");
    Check(await db.ContentTranslations.CountAsync() == 44, "Expected 44 revision translations.");
    Check(await db.ContentItems.AllAsync(x => x.IsDemo && x.State == PublicationState.Draft && x.PublishedRevisionId == null && x.DraftRevisionId != null), "Seed publication state invalid.");
    Check(await db.Users.CountAsync() == 0 && await db.Roles.CountAsync() == 3, "Roles only; no user account should be seeded.");
    Check(await new PublishedContentReader(db).FindAsync(ContentKind.Page, "home", "en") is null, "Draft leaked.");
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

await using(var verify=new NloDbContext(options))
{
    Check(await verify.ContentItems.CountAsync()==22,"Test data was not rolled back.");
    Check(await verify.ServicePreviews.CountAsync()==4,"Preview data changed.");
    Check(await verify.ContentRevisions.CountAsync()==22,"Revision fixture count changed.");
    Console.WriteLine("PASS: all verification transactions rolled back; preview and draft fixtures retained.");
}
