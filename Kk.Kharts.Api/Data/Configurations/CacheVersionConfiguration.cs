using Kk.Kharts.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations
{
    public class CacheVersionConfiguration : IEntityTypeConfiguration<CacheVersion>
    {
        public void Configure(EntityTypeBuilder<CacheVersion> builder)
        {
            builder.ToTable("cache_versions");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.LocalStorageVersion)
                .HasColumnName("local_storage_version")
                .IsRequired()
                .HasDefaultValue(1u);

            builder.Property(e => e.IndexedDbVersion)
                .HasColumnName("indexed_db_version")
                .IsRequired()
                .HasDefaultValue(1u);

            builder.Property(e => e.CacheStorageVersion)
                .HasColumnName("cache_version")
                .IsRequired()
                .HasDefaultValue(1u);

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(100);

            builder.HasIndex(e => e.UpdatedAt)
                .HasDatabaseName("IX_cache_versions_updated_at")
                .IsDescending();
        }
    }
}
