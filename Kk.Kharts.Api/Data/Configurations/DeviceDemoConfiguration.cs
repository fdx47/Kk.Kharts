using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations
{
    public class DeviceDemoConfiguration : IEntityTypeConfiguration<DeviceDemo>
    {
        public void Configure(EntityTypeBuilder<DeviceDemo> builder)
        {

            builder.ToTable("devices_demos");

            builder.Property(p => p.DevEui)
                .HasColumnName("dev_eui")
                .HasColumnOrder(1)
                .HasMaxLength(16);

            builder.Property(p => p.Name)
                .HasColumnName("name")
                .HasColumnOrder(2)
                .HasMaxLength(16);

            builder.Property(p => p.Description)
                .HasColumnName("description")
                .HasColumnOrder(3)
                .HasMaxLength(40);           

            builder.Property(p => p.InstallationLocation)
               .HasColumnName("Installation_location")
               .HasColumnOrder(4)
               .HasMaxLength(50)
               .IsRequired(false);            
        }
    }
}

