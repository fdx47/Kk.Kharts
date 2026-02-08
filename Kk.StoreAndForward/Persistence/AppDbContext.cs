using KK.UG6x.StoreAndForward.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KK.UG6x.StoreAndForward.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<PendingPayload> PendingPayloads { get; set; } = null!;
    public DbSet<AppSetting> AppSettings { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PendingPayload>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DevEui).IsRequired();
            entity.Property(e => e.PayloadJson).IsRequired();
            entity.Property(e => e.EndpointType).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.Category, e.Key }).IsUnique();
        });
    }
}
