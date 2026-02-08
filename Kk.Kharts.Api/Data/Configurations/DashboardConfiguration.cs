using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations
{
    public class DashboardConfiguration : IEntityTypeConfiguration<Dashboard>
    {
        public void Configure(EntityTypeBuilder<Dashboard> builder)
        {
            builder.ToTable("dashboards");

            builder.HasKey(e => new { e.Id });

            builder.Property(ds => ds.StateJson)
                   .HasConversion(
                      v => v,
                      v => v);
        }
    }
}
