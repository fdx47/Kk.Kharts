using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Data.Configurations
{
    public class TechnicianConfiguration : IEntityTypeConfiguration<Technician>
    {
        public void Configure(EntityTypeBuilder<Technician> builder)
        {
            builder.ToTable("technicians");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasMany(t => t.TechnicianDevices)
                .WithOne(td => td.Technician)
                .HasForeignKey(td => td.TechnicianId);
        }
    }
}
