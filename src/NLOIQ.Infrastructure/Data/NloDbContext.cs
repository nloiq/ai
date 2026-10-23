using Microsoft.EntityFrameworkCore;
using NLOIQ.Domain.Entities;
namespace NLOIQ.Infrastructure.Data;
public sealed class NloDbContext(DbContextOptions<NloDbContext> options) : DbContext(options)
{
    public DbSet<ServicePreview> ServicePreviews => Set<ServicePreview>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
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
