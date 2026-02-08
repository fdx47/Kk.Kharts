using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations;

public class AlarmTimePeriodConfiguration : IEntityTypeConfiguration<AlarmTimePeriod>
{
    public void Configure(EntityTypeBuilder<AlarmTimePeriod> builder)
    {
        builder.ToTable("alarm_time_periods");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.AlarmRuleId)
            .HasColumnName("alarm_rule_id")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(p => p.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(p => p.LowValue)
            .HasColumnName("low_value");

        builder.Property(p => p.HighValue)
            .HasColumnName("high_value");

        builder.Property(p => p.IsEnabled)
            .HasColumnName("is_enabled")
            .HasDefaultValue(true);

        builder.Property(p => p.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(1);

        builder.HasOne(p => p.AlarmRule)
            .WithMany(r => r.TimePeriods)
            .HasForeignKey(p => p.AlarmRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.AlarmRuleId, p.DisplayOrder });
    }
}
