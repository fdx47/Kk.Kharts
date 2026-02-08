using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Data.Configurations
{
    public class TechnicianDeviceConfiguration : IEntityTypeConfiguration<TechnicianDevice>
    {
        public void Configure(EntityTypeBuilder<TechnicianDevice> builder)
        {
            builder.ToTable("technician_devices");

            builder.HasKey(td => new { td.TechnicianId, td.DeviceId });

            builder.HasOne(td => td.Technician)
                .WithMany(t => t.TechnicianDevices)
                .HasForeignKey(td => td.TechnicianId);

            builder.HasOne(td => td.Device)
                .WithMany(d => d.TechnicianDevices)
                .HasForeignKey(td => td.DeviceId);
        }
    }
}
