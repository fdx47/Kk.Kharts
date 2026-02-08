using Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations
{

    public class Wet150MultiSensor4Configuration : IEntityTypeConfiguration<Wet150MultiSensor4>
    {
        public void Configure(EntityTypeBuilder<Wet150MultiSensor4> builder)
        {
            builder.ToTable("wet150_multisensor_4");

            builder.HasKey(e => new { e.Timestamp, e.DevEui });

            builder.Property(p => p.Timestamp).HasColumnName("timestamp").HasColumnOrder(0);
            builder.Property(p => p.DevEui).HasColumnName("dev_eui").HasColumnOrder(1).HasMaxLength(16);
            builder.Property(p => p.DeviceId).HasColumnName("device_id").HasColumnOrder(2);

            builder.Property(p => p.Sdi12_1).HasColumnName("sdi12_1").HasColumnOrder(3).HasMaxLength(19);
            builder.Property(p => p.Sdi12_2).HasColumnName("sdi12_2").HasColumnOrder(4).HasMaxLength(19);
            builder.Property(p => p.Sdi12_3).HasColumnName("sdi12_3").HasColumnOrder(5).HasMaxLength(19);
            builder.Property(p => p.Sdi12_4).HasColumnName("sdi12_4").HasColumnOrder(6).HasMaxLength(19);

            builder.Property(p => p.Battery).HasColumnName("batterie").HasColumnOrder(7);

            builder.HasOne(e => e.Device)
                 .WithMany(d => d.Wet150MultiSensors4)
                 .HasForeignKey(e => e.DeviceId)
                 .IsRequired(false) // Permitir que Device seja opcional
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}