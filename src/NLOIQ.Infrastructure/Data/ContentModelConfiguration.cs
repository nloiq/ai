using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NLOIQ.Domain.Content;
using NLOIQ.Infrastructure.Identity;
namespace NLOIQ.Infrastructure.Data;
internal static class ContentModelConfiguration
{
    private static EntityTypeBuilder<T> Owned<T>(ModelBuilder model, string table) where T : class, IRevisionOwned
    {
        var entity = model.Entity<T>();
        entity.ToTable(table);
        entity.HasOne(x => x.Revision).WithMany().HasForeignKey(x => x.RevisionId).OnDelete(DeleteBehavior.Restrict);
        return entity;
    }
    private static EntityTypeBuilder<T> Snapshot<T>(ModelBuilder model, string table) where T : class, IContentSnapshot
    {
        var entity = Owned<T>(model, table);
        entity.HasKey(x => x.RevisionId);
        entity.Ignore(x => x.Kind);
        return entity;
    }
    private static void Culture<T>(EntityTypeBuilder<T> entity, string table) where T : class
    {
        entity.Property<string>("Culture").HasMaxLength(2).IsRequired();
        entity.ToTable(table, options => options.HasCheckConstraint("CK_" + table + "_Culture", "`Culture` IN ('en','ar')"));
    }
    public static void Configure(ModelBuilder model)
    {
        model.Entity<ApplicationUser>().Property(x => x.DisplayName).HasMaxLength(160).IsRequired();
        var roots = model.Entity<ContentItem>();
        roots.ToTable("ContentItems", table => {
            table.HasCheckConstraint("CK_ContentItems_State", "`State` IN ('Draft','Published','Archived')");
            table.HasCheckConstraint("CK_ContentItems_PublishedPointer", "`State` <> 'Published' OR `PublishedRevisionId` IS NOT NULL");
            table.HasCheckConstraint("CK_ContentItems_Version", "`Version` > 0");
        });
        roots.HasKey(x => x.Id); roots.Property(x => x.Id).ValueGeneratedOnAdd().HasAnnotation("MySQL:ValueGenerationStrategy", MySql.EntityFrameworkCore.Metadata.MySQLValueGenerationStrategy.IdentityColumn);
        roots.Property(x => x.Kind).HasConversion<string>().HasMaxLength(32);
        roots.Property(x => x.State).HasConversion<string>().HasMaxLength(16);
        roots.Property(x => x.Key).HasMaxLength(80).UseCollation("utf8mb4_bin").IsRequired();
        roots.HasIndex(x => new { x.Kind, x.Key }).IsUnique();
        roots.HasIndex(x => new { x.Kind, x.State });
        roots.Property(x => x.Version).IsConcurrencyToken();
        roots.HasOne(x => x.DraftRevision).WithMany().HasForeignKey(x => new { x.Id, x.DraftRevisionId })
            .HasPrincipalKey(x => new { x.ContentItemId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        roots.HasOne(x => x.PublishedRevision).WithMany().HasForeignKey(x => new { x.Id, x.PublishedRevisionId })
            .HasPrincipalKey(x => new { x.ContentItemId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        var revisions = model.Entity<ContentRevision>();
        revisions.ToTable("ContentRevisions", table => table.HasCheckConstraint("CK_ContentRevisions_Number", "`RevisionNumber` > 0 AND `SchemaVersion` > 0"));
        revisions.HasKey(x => x.Id); revisions.Property(x => x.Id).ValueGeneratedOnAdd();
        revisions.HasAlternateKey(x => new { x.ContentItemId, x.Id });
        revisions.HasIndex(x => new { x.ContentItemId, x.RevisionNumber }).IsUnique();
        revisions.Property(x => x.ChangeNote).HasMaxLength(500);
        revisions.HasOne(x => x.ContentItem).WithMany().HasForeignKey(x => x.ContentItemId).OnDelete(DeleteBehavior.Restrict);
        revisions.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        revisions.HasOne<ContentRevision>().WithMany().HasForeignKey(x => new { x.ContentItemId, x.SupersedesRevisionId })
            .HasPrincipalKey(x => new { x.ContentItemId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        var translations = model.Entity<ContentTranslation>();
        translations.ToTable("ContentTranslations"); translations.HasKey(x => new { x.RevisionId, x.Culture });
        translations.HasOne(x => x.Revision).WithMany(x => x.Translations).HasForeignKey(x => x.RevisionId).OnDelete(DeleteBehavior.Restrict);
        Culture(translations, "ContentTranslations");
        translations.Property(x => x.Title).HasMaxLength(200).IsRequired();
        translations.Property(x => x.Summary).HasMaxLength(2000).IsRequired();
        translations.Property(x => x.Body).HasMaxLength(16000).HasColumnType("longtext").IsRequired();
        translations.Property(x => x.Slug).HasMaxLength(160).UseCollation("utf8mb4_bin");
        translations.Property(x => x.PrimaryButtonLabel).HasMaxLength(120);
        translations.Property(x => x.SecondaryButtonLabel).HasMaxLength(120);
        var pages = Snapshot<ContentPage>(model, "Pages"); pages.Property(x => x.LayoutKey).HasMaxLength(40).IsRequired();
        var sections = Snapshot<ContentSection>(model, "Sections");
        sections.Property(x => x.SectionKind).HasConversion<string>().HasMaxLength(20);
        sections.Property(x => x.LayoutKey).HasMaxLength(40).IsRequired(); sections.Property(x => x.Background).HasMaxLength(20).IsRequired();
        sections.Property(x => x.ParallaxIntensity).HasPrecision(3, 2);
        sections.Property(x => x.PrimaryButtonTarget).HasMaxLength(500); sections.Property(x => x.SecondaryButtonTarget).HasMaxLength(500);
        sections.ToTable("Sections", table => table.HasCheckConstraint("CK_Sections_Parallax", "`ParallaxIntensity` BETWEEN 0 AND 1"));
        var projects = Snapshot<ContentProject>(model, "Projects"); projects.Property(x => x.YouTubeVideoId).HasMaxLength(11);
        projects.ToTable("Projects", table => table.HasCheckConstraint("CK_Projects_Order", "`DisplayOrder` >= 0"));
        projects.HasIndex(x => new { x.IsFeatured, x.DisplayOrder });
        projects.HasOne(x => x.CoverMediaUsage).WithMany().HasForeignKey(x => new { x.RevisionId, x.CoverMediaUsageId }).HasPrincipalKey(x => new { x.RevisionId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        Snapshot<ProjectCategory>(model, "ProjectCategories");
        var services = Snapshot<ContentService>(model, "Services"); services.Property(x => x.IconKey).HasMaxLength(32).IsRequired();
        services.HasIndex(x => x.DisplayOrder);
        services.ToTable("Services", table => table.HasCheckConstraint("CK_Services_Order", "`DisplayOrder` >= 0"));
        services.HasOne(x => x.MediaUsage).WithMany().HasForeignKey(x => new { x.RevisionId, x.MediaUsageId }).HasPrincipalKey(x => new { x.RevisionId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        var pageSections = Owned<PageSection>(model, "PageSections"); pageSections.HasKey(x => new { x.RevisionId, x.SectionRevisionId });
        pageSections.HasOne<ContentPage>().WithMany().HasForeignKey(x => x.RevisionId).OnDelete(DeleteBehavior.Restrict);
        pageSections.HasOne(x => x.Section).WithMany().HasForeignKey(x => x.SectionRevisionId).OnDelete(DeleteBehavior.Restrict);
        pageSections.HasIndex(x => new { x.RevisionId, x.DisplayOrder }).IsUnique();
        pageSections.ToTable("PageSections", table => table.HasCheckConstraint("CK_PageSections_Order", "`DisplayOrder` >= 0"));
        var categories = Owned<ProjectCategoryAssignment>(model, "ProjectCategoryAssignments"); categories.HasKey(x => new { x.RevisionId, x.CategoryRevisionId });
        categories.HasOne<ContentProject>().WithMany().HasForeignKey(x => x.RevisionId).OnDelete(DeleteBehavior.Restrict);
        categories.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryRevisionId).OnDelete(DeleteBehavior.Restrict);
        var gallery = Owned<ProjectMedia>(model, "ProjectMedia"); gallery.HasKey(x => new { x.RevisionId, x.MediaUsageId });
        gallery.HasOne<ContentProject>().WithMany().HasForeignKey(x => x.RevisionId).OnDelete(DeleteBehavior.Restrict);
        gallery.HasOne(x => x.MediaUsage).WithMany().HasForeignKey(x => new { x.RevisionId, x.MediaUsageId }).HasPrincipalKey(x => new { x.RevisionId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        gallery.HasIndex(x => new { x.RevisionId, x.DisplayOrder }).IsUnique();
        var nav = Snapshot<NavigationSettings>(model, "NavigationSettings"); nav.Property(x => x.Location).HasMaxLength(32).IsRequired();
        var navItems = Owned<NavigationItem>(model, "NavigationItems"); navItems.HasKey(x => x.Id); navItems.HasAlternateKey(x => new { x.RevisionId, x.Id });
        navItems.HasOne<NavigationSettings>().WithMany().HasForeignKey(x => x.RevisionId).OnDelete(DeleteBehavior.Restrict);
        navItems.HasOne(x => x.Parent).WithMany().HasForeignKey(x => new { x.RevisionId, x.ParentItemId }).HasPrincipalKey(x => new { x.RevisionId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        navItems.HasOne<ContentItem>().WithMany().HasForeignKey(x => x.TargetContentItemId).OnDelete(DeleteBehavior.Restrict);
        navItems.Property(x => x.Href).HasMaxLength(500); navItems.HasIndex(x => new { x.RevisionId, x.DisplayOrder }).IsUnique();
        var navText = Owned<NavigationTranslation>(model, "NavigationTranslations"); navText.HasKey(x => new { x.RevisionId, x.NavigationItemId, x.Culture });
        navText.HasOne(x => x.NavigationItem).WithMany().HasForeignKey(x => new { x.RevisionId, x.NavigationItemId }).HasPrincipalKey(x => new { x.RevisionId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        navText.Property(x => x.Label).HasMaxLength(120).IsRequired(); Culture(navText, "NavigationTranslations");
        var contact = Snapshot<ContactSettings>(model, "ContactSettings"); contact.Property(x => x.TimeZoneId).HasMaxLength(80).IsRequired();
        var details = Owned<ContactDetail>(model, "ContactDetails"); details.HasKey(x => x.Id); details.HasAlternateKey(x => new { x.RevisionId, x.Id });
        details.HasOne<ContactSettings>().WithMany().HasForeignKey(x => x.RevisionId).OnDelete(DeleteBehavior.Restrict);
        details.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20); details.Property(x => x.Value).HasMaxLength(1000).IsRequired();
        details.HasIndex(x => new { x.RevisionId, x.DisplayOrder }).IsUnique();
        var contactText = Owned<ContactTranslation>(model, "ContactTranslations"); contactText.HasKey(x => new { x.RevisionId, x.ContactDetailId, x.Culture });
        contactText.HasOne(x => x.ContactDetail).WithMany().HasForeignKey(x => new { x.RevisionId, x.ContactDetailId }).HasPrincipalKey(x => new { x.RevisionId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        contactText.Property(x => x.Label).HasMaxLength(120).IsRequired(); contactText.Property(x => x.LocalizedValue).HasMaxLength(1000); Culture(contactText, "ContactTranslations");
        var hours = Owned<BusinessHours>(model, "BusinessHours"); hours.HasKey(x => x.Id);
        var clockConverter = new ValueConverter<TimeOnly, TimeSpan>(value => value.ToTimeSpan(), value => TimeOnly.FromTimeSpan(value));
        hours.Property(x => x.OpensAt).HasConversion(clockConverter);
        hours.Property(x => x.ClosesAt).HasConversion(clockConverter);
        hours.HasOne<ContactSettings>().WithMany().HasForeignKey(x => x.RevisionId).OnDelete(DeleteBehavior.Restrict);
        hours.HasIndex(x => new { x.RevisionId, x.DayOfWeek }).IsUnique();
        hours.ToTable("BusinessHours", table => { table.HasCheckConstraint("CK_BusinessHours_Day", "`DayOfWeek` BETWEEN 0 AND 6"); table.HasCheckConstraint("CK_BusinessHours_Times", "(`IsClosed` = 1 AND `OpensAt` IS NULL AND `ClosesAt` IS NULL) OR (`IsClosed` = 0 AND `OpensAt` IS NOT NULL AND `ClosesAt` IS NOT NULL)"); });
        var social = Owned<SocialLink>(model, "SocialLinks"); social.HasKey(x => x.Id);
        social.Property(x => x.Platform).HasMaxLength(32).IsRequired(); social.Property(x => x.Url).HasMaxLength(1000).IsRequired();
        social.HasIndex(x => new { x.RevisionId, x.Platform }).IsUnique();
        Snapshot<FooterSettings>(model, "FooterSettings");
        var site = Snapshot<SiteSettings>(model, "SiteSettings"); site.Property(x => x.SiteName).HasMaxLength(160).IsRequired();
        site.Property(x => x.DefaultCulture).HasMaxLength(2).IsRequired(); site.Property(x => x.DefaultTheme).HasMaxLength(8).IsRequired(); site.Property(x => x.PublicBaseUrl).HasMaxLength(500);
        site.ToTable("SiteSettings", table => { table.HasCheckConstraint("CK_SiteSettings_Culture", "`DefaultCulture` IN ('en','ar')"); table.HasCheckConstraint("CK_SiteSettings_Theme", "`DefaultTheme` IN ('light','dark','system')"); });
        site.HasOne(x => x.LogoMediaUsage).WithMany().HasForeignKey(x => new { x.RevisionId, x.LogoMediaUsageId }).HasPrincipalKey(x => new { x.RevisionId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        site.HasOne(x => x.FaviconMediaUsage).WithMany().HasForeignKey(x => new { x.RevisionId, x.FaviconMediaUsageId }).HasPrincipalKey(x => new { x.RevisionId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        var theme = Snapshot<ThemeSettings>(model, "ThemeSettings");
        foreach (var property in typeof(ThemeSettings).GetProperties().Where(x => x.PropertyType == typeof(string))) theme.Property(property.Name).HasColumnType("char(7)").IsRequired();
        var media = model.Entity<MediaAsset>(); media.ToTable("MediaAssets"); media.HasKey(x => x.Id);
        media.Property(x => x.StorageProvider).HasMaxLength(32).IsRequired(); media.Property(x => x.StorageKey).HasMaxLength(300).UseCollation("utf8mb4_bin").IsRequired();
        media.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired(); media.Property(x => x.ContentType).HasMaxLength(100).IsRequired(); media.Property(x => x.Sha256).HasColumnType("char(64)").IsRequired();
        media.HasIndex(x => new { x.StorageProvider, x.StorageKey }).IsUnique(); media.HasIndex(x => x.Sha256);
        media.ToTable("MediaAssets", table => { table.HasCheckConstraint("CK_MediaAssets_Bytes", "`ByteSize` > 0"); table.HasCheckConstraint("CK_MediaAssets_Dimensions", "(`Width` IS NULL OR `Width` > 0) AND (`Height` IS NULL OR `Height` > 0)"); });
        var assetText = model.Entity<MediaAssetTranslation>(); assetText.ToTable("MediaAssetTranslations"); assetText.HasKey(x => new { x.MediaAssetId, x.Culture });
        assetText.HasOne(x => x.MediaAsset).WithMany().HasForeignKey(x => x.MediaAssetId).OnDelete(DeleteBehavior.Restrict);
        assetText.Property(x => x.AltText).HasMaxLength(500).IsRequired(); Culture(assetText, "MediaAssetTranslations");
        var usage = Owned<RevisionMedia>(model, "RevisionMedia"); usage.HasKey(x => x.Id); usage.HasAlternateKey(x => new { x.RevisionId, x.Id });
        usage.HasOne(x => x.MediaAsset).WithMany().HasForeignKey(x => x.MediaAssetId).OnDelete(DeleteBehavior.Restrict);
        usage.Property(x => x.UsageKey).HasMaxLength(40).IsRequired(); usage.HasIndex(x => new { x.RevisionId, x.UsageKey }).IsUnique();
        var usageText = Owned<RevisionMediaTranslation>(model, "RevisionMediaTranslations"); usageText.HasKey(x => new { x.RevisionId, x.RevisionMediaId, x.Culture });
        usageText.HasOne(x => x.RevisionMedia).WithMany().HasForeignKey(x => new { x.RevisionId, x.RevisionMediaId }).HasPrincipalKey(x => new { x.RevisionId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        usageText.Property(x => x.AltText).HasMaxLength(500).IsRequired(); usageText.Property(x => x.Caption).HasMaxLength(1000); Culture(usageText, "RevisionMediaTranslations");
        var seo = Owned<SeoMetadata>(model, "SeoMetadata"); seo.HasKey(x => new { x.RevisionId, x.Culture }); Culture(seo, "SeoMetadata");
        seo.HasOne(x => x.Translation).WithOne().HasForeignKey<SeoMetadata>(x => new { x.RevisionId, x.Culture }).OnDelete(DeleteBehavior.Restrict);
        seo.Property(x => x.Title).HasMaxLength(200).IsRequired(); seo.Property(x => x.Description).HasMaxLength(320).IsRequired();
        seo.Property(x => x.OpenGraphTitle).HasMaxLength(200); seo.Property(x => x.OpenGraphDescription).HasMaxLength(320); seo.Property(x => x.CanonicalPath).HasMaxLength(500);
        seo.HasOne(x => x.OpenGraphMediaUsage).WithMany().HasForeignKey(x => new { x.RevisionId, x.OpenGraphMediaUsageId }).HasPrincipalKey(x => new { x.RevisionId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        var routes = model.Entity<PublishedRoute>(); routes.ToTable("PublishedRoutes"); routes.HasKey(x => x.Id); Culture(routes, "PublishedRoutes");
        routes.Property(x => x.Scope).HasMaxLength(32).UseCollation("utf8mb4_bin").IsRequired(); routes.Property(x => x.Slug).HasMaxLength(160).UseCollation("utf8mb4_bin").IsRequired();
        routes.HasIndex(x => new { x.Culture, x.Scope, x.Slug }).IsUnique(); routes.HasIndex(x => new { x.ContentItemId, x.Culture }).IsUnique();
        routes.HasOne<ContentRevision>().WithMany().HasForeignKey(x => new { x.ContentItemId, x.RevisionId }).HasPrincipalKey(x => new { x.ContentItemId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        var schedule = model.Entity<PublicationSchedule>(); schedule.ToTable("PublicationSchedules"); schedule.HasKey(x => x.Id);
        schedule.Property(x => x.State).HasConversion<string>().HasMaxLength(16); schedule.Property(x => x.Version).IsConcurrencyToken(); schedule.Property(x => x.FailureCode).HasMaxLength(80);
        schedule.HasIndex(x => new { x.State, x.ScheduledUtc }); schedule.HasIndex(x => new { x.ContentItemId, x.RevisionId });
        schedule.HasOne<ContentRevision>().WithMany().HasForeignKey(x => new { x.ContentItemId, x.RevisionId }).HasPrincipalKey(x => new { x.ContentItemId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        schedule.Property(x=>x.RequestedByLabel).HasMaxLength(160).HasDefaultValue("");
        schedule.Property(x=>x.CreatedUtc).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        schedule.HasOne<ApplicationUser>().WithMany().HasForeignKey(x=>x.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
        var audit = model.Entity<AuditLog>(); audit.ToTable("AuditLogs"); audit.HasKey(x => x.Id);
        audit.Property(x => x.ActorLabel).HasMaxLength(160).IsRequired(); audit.Property(x => x.Action).HasMaxLength(80).IsRequired(); audit.Property(x => x.DetailsJson).HasColumnType("longtext");
        audit.HasIndex(x => x.OccurredUtc); audit.HasIndex(x => new { x.ContentItemId, x.OccurredUtc }); audit.HasIndex(x => new { x.ActorUserId, x.OccurredUtc });
        audit.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        audit.HasOne<ContentItem>().WithMany().HasForeignKey(x => x.ContentItemId).OnDelete(DeleteBehavior.Restrict);
        audit.HasOne<ContentRevision>().WithMany().HasForeignKey(x => new { x.ContentItemId, x.RevisionId }).HasPrincipalKey(x => new { x.ContentItemId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        foreach (var entity in model.Model.GetEntityTypes()) foreach (var property in entity.GetProperties())
            if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                model.Entity(entity.ClrType).Property(property.Name).HasColumnType("datetime(6)").HasConversion(new ValueConverter<DateTime, DateTime>(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc)));
    }
}
