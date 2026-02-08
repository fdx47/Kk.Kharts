using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Data.Configurations
{
    public class AlarmRuleConfiguration : IEntityTypeConfiguration<AlarmRule>
    {
        public void Configure(EntityTypeBuilder<AlarmRule> builder)
        {
            // Nome da tabela
            builder.ToTable("alarm_rules");

            // Chave primária
            builder.HasKey(ar => ar.Id);

            // Colunas
            builder.Property(ar => ar.Id)
                .HasColumnName("id")
                .HasColumnOrder(1);

            builder.Property(p => p.DevEui).HasColumnName("dev_eui").HasColumnOrder(2).HasMaxLength(16);

            builder.Property(ar => ar.DeviceId)
                .HasColumnName("device_id")
                .HasColumnOrder(3);

            builder.Property(ar => ar.Description)
                .HasColumnName("description")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnOrder(4);

            builder.Property(ar => ar.PropertyName)
                .HasColumnName("property_name")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnOrder(5);

            builder.Property(ar => ar.LowValue)
                .HasColumnName("low_value")
                .HasColumnOrder(6);

            builder.Property(ar => ar.HighValue)
                .HasColumnName("high_value")
                 .HasColumnOrder(7);

            builder.Property(ar => ar.Hysteresis)
                .HasColumnName("hysteresis")
                .HasColumnOrder(8);

            builder.Property(ar => ar.Enabled)
                .HasColumnName("enabled")
                .HasDefaultValue(true)
                .HasColumnOrder(9);

            builder.Property(ar => ar.IsAlarmActive)
          .HasColumnName("is_alarm_active")
          .HasDefaultValue(false);

            builder.Property(ar => ar.IsAlarmHandled)
         .HasColumnName("is_alarm_Handled")
         .HasDefaultValue(false);

            builder.Property(ar => ar.ActiveThresholdType)
                   .HasColumnName("active_threshold_type")
                   .HasMaxLength(10); // "Low" ou "High"

            //   //Relacionamento com User ---
            //    builder.HasOne(ar => ar.User) // Uma AlarmRule tem UM User
            //        .WithMany(u => u.AlarmRules) // Um User pode ter MUITAS AlarmRules
            //        .HasForeignKey(ar => ar.UserId) // A chave estrangeira é UserId na AlarmRule
            //        .OnDelete(DeleteBehavior.NoAction);
            //}


            // Relacionamento com Device
            builder.HasOne(ar => ar.Device)
                .WithMany(d => d.AlarmRules) // Assumindo que Device tem uma List<AlarmRule> chamada AlarmRules
                .HasForeignKey(ar => ar.DeviceId)
                .OnDelete(DeleteBehavior.NoAction); // <-- Mantenha como NoAction ou Restrict

            
        }
    }
}
