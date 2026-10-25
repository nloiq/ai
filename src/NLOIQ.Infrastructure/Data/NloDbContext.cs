using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NLOIQ.Domain.Content;
using NLOIQ.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using NLOIQ.Domain.Entities;
namespace NLOIQ.Infrastructure.Data;
public sealed class NloDbContext(DbContextOptions<NloDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<ContactInquiry> ContactInquiries => Set<ContactInquiry>();
    public DbSet<ServicePreview> ServicePreviews => Set<ServicePreview>();
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<ContentRevision> ContentRevisions => Set<ContentRevision>();
    public DbSet<ContentTranslation> ContentTranslations => Set<ContentTranslation>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ContentWriteGuard.Validate(this);
        ContentWriteGuard.ValidatePublicationAsync(this, CancellationToken.None).GetAwaiter().GetResult();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ContentWriteGuard.Validate(this);
        await ContentWriteGuard.ValidatePublicationAsync(this, cancellationToken);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        ContentModelConfiguration.Configure(builder);
        builder.Entity<ContactInquiry>(entity => {
            entity.ToTable("ContactInquiries");entity.HasKey(x=>x.Id);
            entity.HasIndex(x=>x.SubmissionId).IsUnique();entity.HasIndex(x=>new{x.IsRead,x.CreatedUtc});
            entity.Property(x=>x.Culture).HasMaxLength(2).IsRequired();entity.Property(x=>x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x=>x.Email).HasMaxLength(254).IsRequired();entity.Property(x=>x.Phone).HasMaxLength(40).IsRequired();entity.Property(x=>x.Company).HasMaxLength(160).IsRequired();
            entity.Property(x=>x.Service).HasMaxLength(30).IsRequired();entity.Property(x=>x.Message).HasMaxLength(4000).IsRequired();
        });
        builder.Entity<ServicePreview>(entity => {
            entity.ToTable("ServicePreviews"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => x.Key).IsUnique(); entity.HasIndex(x => x.DisplayOrder);
            entity.HasMany(x => x.Translations).WithOne(x => x.ServicePreview).HasForeignKey(x => x.ServicePreviewId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<ServicePreviewTranslation>(entity => {
            entity.ToTable("ServicePreviewTranslations"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Culture).HasMaxLength(2).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(1000).IsRequired();
            entity.HasIndex(x => new { x.ServicePreviewId, x.Culture }).IsUnique();
        });
    }
}
