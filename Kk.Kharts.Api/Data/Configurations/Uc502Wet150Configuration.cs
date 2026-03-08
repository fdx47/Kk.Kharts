using Kk.Kharts.Shared.Entities.UC502;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations
{

    public class Uc502Wet150Configuration : IEntityTypeConfiguration<Uc502Wet150>
    {
        public void Configure(EntityTypeBuilder<Uc502Wet150> builder)
        {
            builder.ToTable("wets_150");

            builder.HasKey(e => new { e.Timestamp, e.DevEui });
            builder.HasIndex(e => new { e.DevEui, e.Timestamp })
                .HasDatabaseName("IX_wets_150_dev_eui_timestamp");

            builder.Property(p => p.Timestamp).HasColumnName("timestamp").HasColumnOrder(0);
            builder.Property(p => p.DevEui).HasColumnName("dev_eui").HasColumnOrder(1).HasMaxLength(16);
            builder.Property(p => p.DeviceId).HasColumnName("device_id").HasColumnOrder(2);

            builder.Property(p => p.Permittivite).HasColumnName("permittivite").HasColumnOrder(3);
            builder.Property(p => p.ECb).HasColumnName("ec_bulk").HasColumnOrder(4);
            builder.Property(p => p.SoilTemperature).HasColumnName("soil_temperature").HasColumnOrder(5);

            builder.Property(p => p.MineralVWC).HasColumnName("mineral_vwc").HasColumnOrder(6);
            builder.Property(p => p.MineralECp).HasColumnName("mineral_ecp").HasColumnOrder(7);

            builder.Property(p => p.OrganicVWC).HasColumnName("organic_vwc").HasColumnOrder(8);
            builder.Property(p => p.OrganicECp).HasColumnName("organic_ecp").HasColumnOrder(9);

            builder.Property(p => p.PeatMixVWC).HasColumnName("peatmix_vwc").HasColumnOrder(10);
            builder.Property(p => p.PeatMixECp).HasColumnName("peatmix_ecp").HasColumnOrder(11);

            builder.Property(p => p.CoirVWC).HasColumnName("coir_vwc").HasColumnOrder(12);
            builder.Property(p => p.CoirECp).HasColumnName("coir_ecp").HasColumnOrder(13);

            builder.Property(p => p.MinWoolVWC).HasColumnName("minwool_vwc").HasColumnOrder(14);
            builder.Property(p => p.MinWoolECp).HasColumnName("minwool_ecp").HasColumnOrder(15);

            builder.Property(p => p.PerliteVWC).HasColumnName("perlite_vwc").HasColumnOrder(16);
            builder.Property(p => p.PerliteECp).HasColumnName("perlite_ecp").HasColumnOrder(17);

            builder.Property(p => p.Battery).HasColumnName("batterie").HasColumnOrder(18);

            builder.HasOne(e => e.Device)
                 .WithMany(d => d.Uc502Wet150s)
                 .HasForeignKey(e => e.DeviceId)
                 .IsRequired(false) // Permitir que Device seja opcional
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}