using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLOIQ.Domain.Content;
namespace NLOIQ.Infrastructure.Data;
public sealed class ContentArchitectureSeeder(NloDbContext database)
{
    public async Task<int> SeedAsync(string webRootPath, string? suppliedVideoId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        foreach (var role in new[] { "SuperAdmin", "Editor", "Viewer" })
            if (!await database.Roles.AnyAsync(x => x.NormalizedName == role.ToUpperInvariant(), cancellationToken))
                database.Roles.Add(new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = role, NormalizedName = role.ToUpperInvariant(), ConcurrencyStamp = Guid.NewGuid().ToString() });
        // Seed is a non-overwriting initial fixture, not a content restoration mechanism.
        if (await database.ContentItems.AnyAsync(x => x.Kind == ContentKind.Page && x.Key == "home", cancellationToken))
        {
            await database.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return 0;
        }
        if (await database.ContentItems.AnyAsync(cancellationToken)) throw new InvalidOperationException("Initial content fixtures conflict with existing content; no data was overwritten.");
        var created = new List<(ContentItem Item, ContentRevision Revision)>();
        ContentRevision Revision(ContentKind kind, string key, string english, string arabic, string summaryEn = "Development sample — requires company approval.", string summaryAr = "محتوى تطوير تجريبي — يحتاج إلى اعتماد الشركة.")
        {
            var item = new ContentItem { Kind = kind, Key = key, IsDemo = true };
            var revision = new ContentRevision { ContentItem = item, RevisionNumber = 1, ChangeNote = "Development fixture, not approved public content." };
            revision.Translations = [new() { Revision = revision, Culture = "en", Title = english, Summary = summaryEn, Slug = kind is ContentKind.Page or ContentKind.Project ? key : null }, new() { Revision = revision, Culture = "ar", Title = arabic, Summary = summaryAr, Slug = kind is ContentKind.Page or ContentKind.Project ? key : null }];
            database.Add(revision); created.Add((item, revision)); return revision;
        }
        var home = new ContentPage { Revision = Revision(ContentKind.Page, "home", "NLO IQ", "NLO IQ") };
        database.Add(home);
        var sectionSpecs = new[] {
            ("hero", SectionKind.Hero, "Engineering the invisible. Connecting what matters.", "نهندس ما لا يُرى. ونصل ما يهمّك."),
            ("about", SectionKind.About, "Where precision meets possibility.", "حيث تلتقي الدقّة بالإمكانات."),
            ("projects", SectionKind.Projects, "Connections, made visible.", "روابط تتّضح صورتها."),
            ("services", SectionKind.Services, "Systems, working together.", "أنظمة تعمل معاً."),
            ("contact", SectionKind.Contact, "Have a system in mind?", "هل تفكّر في نظام جديد؟")
        };
        for (var index = 0; index < sectionSpecs.Length; index++)
        {
            var spec = sectionSpecs[index];
            var section = new ContentSection { Revision = Revision(ContentKind.Section, spec.Item1, spec.Item3, spec.Item4), SectionKind = spec.Item2, LayoutKey = "folio", Background = spec.Item2 == SectionKind.Contact ? "evergreen" : "paper" };
            database.Add(section);
            database.Add(new PageSection { Revision = home.Revision, Section = section, DisplayOrder = index });
        }
        var categoryTitles = new[] { ("low-current", "Low-current engineering", "هندسة التيار الخفيف"), ("software", "Software solutions", "الحلول البرمجية"), ("scada", "SCADA system integration", "تكامل أنظمة SCADA"), ("automation", "System automation", "أتمتة الأنظمة") };
        var categories = new Dictionary<string, ProjectCategory>();
        foreach (var spec in categoryTitles)
        {
            var category = new ProjectCategory { Revision = Revision(ContentKind.ProjectCategory, spec.Item1, spec.Item2, spec.Item3) };
            categories.Add(spec.Item1, category); database.Add(category);
        }
        var previews = await database.ServicePreviews.AsNoTracking().Include(x => x.Translations).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
        foreach (var preview in previews)
        {
            var english = preview.Translations.Single(x => x.Culture == "en"); var arabic = preview.Translations.Single(x => x.Culture == "ar");
            var revision = Revision(ContentKind.Service, preview.Key, english.Title, arabic.Title, english.Summary, arabic.Summary);
            database.Add(new ContentService { Revision = revision, DisplayOrder = preview.DisplayOrder, IconKey = preview.Key switch { "software" => "code", "scada" => "monitor", "automation" => "logic", _ => "network" } });
        }
        var projectSpecs = new[] {
            ("network-study", "Connected infrastructure", "بنية تحتية مترابطة", "network-study.svg", 840, 600, "low-current"),
            ("scada-study", "A clearer view of control", "رؤية أوضح للتحكّم", "scada-study.svg", 600, 720, "scada"),
            ("automation-study", "Logic into action", "من المنطق إلى التنفيذ", "automation-study.svg", 1000, 480, "automation")
        };
        for (var index = 0; index < projectSpecs.Length; index++)
        {
            var spec = projectSpecs[index];
            var bytes = await File.ReadAllBytesAsync(Path.Combine(webRootPath, "images", spec.Item4), cancellationToken);
            var asset = new MediaAsset { StorageProvider = "Bundled", StorageKey = "images/" + spec.Item4, OriginalFileName = spec.Item4, ContentType = "image/svg+xml", ByteSize = bytes.LongLength, Width = spec.Item5, Height = spec.Item6, Sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(), IsDemo = true };
            database.Add(asset);
            var revision = Revision(ContentKind.Project, spec.Item1, spec.Item2, spec.Item3, "Illustrative study, not a completed client project.", "دراسة توضيحية، وليست مشروع عميل منجزاً.");
            var media = new RevisionMedia { Revision = revision, MediaAsset = asset, UsageKey = "cover", DisplayOrder = 0 };
            database.Add(media);
            foreach (var translation in revision.Translations)
            {
                database.Add(new MediaAssetTranslation { MediaAsset = asset, Culture = translation.Culture, AltText = translation.Title });
                database.Add(new RevisionMediaTranslation { Revision = revision, RevisionMedia = media, Culture = translation.Culture, AltText = translation.Title, Caption = translation.Summary });
                database.Add(new SeoMetadata { Revision = revision, Translation = translation, Culture = translation.Culture, Title = translation.Title, Description = translation.Summary, OpenGraphMediaUsage = media, NoIndex = true });
            }
            var project = new ContentProject { Revision = revision, DisplayOrder = index, IsFeatured = index == 0, YouTubeVideoId = index == 0 ? suppliedVideoId : null, CoverMediaUsage = media };
            database.Add(project);
            database.Add(new ProjectMedia { Revision = revision, MediaUsage = media, DisplayOrder = 0 });
            database.Add(new ProjectCategoryAssignment { Revision = revision, Category = categories[spec.Item7] });
        }
        var navigation = new NavigationSettings { Revision = Revision(ContentKind.Navigation, "main", "Main navigation", "التنقّل الرئيسي") };
        database.Add(navigation);
        var links = new[] { ("#about", "About", "من نحن"), ("#projects", "Projects", "المشاريع"), ("#expertise", "Expertise", "التخصّصات"), ("#contact", "Contact", "تواصل معنا") };
        for (var index = 0; index < links.Length; index++)
        {
            var spec = links[index]; var item = new NavigationItem { Revision = navigation.Revision, Href = spec.Item1, DisplayOrder = index };
            database.Add(item);
            database.Add(new NavigationTranslation { Revision = navigation.Revision, NavigationItem = item, Culture = "en", Label = spec.Item2 });
            database.Add(new NavigationTranslation { Revision = navigation.Revision, NavigationItem = item, Culture = "ar", Label = spec.Item3 });
        }
        database.Add(new ContactSettings { Revision = Revision(ContentKind.Contact, "primary", "Contact information pending", "بيانات التواصل قيد الإضافة") });
        database.Add(new FooterSettings { Revision = Revision(ContentKind.Footer, "main", "NLO IQ", "NLO IQ") });
        database.Add(new SiteSettings { Revision = Revision(ContentKind.SiteSettings, "site", "Site settings", "إعدادات الموقع") });
        database.Add(new ThemeSettings { Revision = Revision(ContentKind.ThemeSettings, "brand", "Brand appearance", "مظهر العلامة") });
        await database.SaveChangesAsync(cancellationToken);
        foreach (var (item, revision) in created) item.DraftRevisionId = revision.Id;
        database.AuditLogs.Add(new AuditLog { ActorLabel = "Development seed", Action = "SeedContentArchitecture", DetailsJson = "{\"demoOnly\":true}" });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return created.Count;
    }
}
