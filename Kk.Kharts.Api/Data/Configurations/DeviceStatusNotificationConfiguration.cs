using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations
{
    public sealed class DeviceStatusNotificationConfiguration : IEntityTypeConfiguration<DeviceStatusNotification>
    {
        public void Configure(EntityTypeBuilder<DeviceStatusNotification> builder)
        {
            builder.ToTable("device_status_notifications");

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasColumnOrder(1)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();

            builder.Property(p => p.DeviceId)
                .HasColumnName("device_id")
                .HasColumnOrder(2);

            builder.Property(p => p.DevEui)
                .HasColumnName("dev_eui")
                .HasColumnOrder(3)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(p => p.MessageId)
                .HasColumnName("message_id")
                .HasColumnOrder(4)
                .IsRequired();

            builder.Property(p => p.Type)
                .HasColumnName("type")
                .HasColumnOrder(5)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(p => p.SentAtUtc)
                .HasColumnName("sent_at_utc")
                .HasColumnOrder(6)
                .IsRequired();

            builder.Property(p => p.IsActive)
                .HasColumnName("is_active")
                .HasColumnOrder(7)
                .HasDefaultValue(true);

            builder.HasOne<Device>()
                .WithMany()
                .HasForeignKey(p => p.DeviceId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(p => p.DevEui);
            builder.HasIndex(p => p.DeviceId);
            builder.HasIndex(p => p.IsActive);
        }
    }
}
