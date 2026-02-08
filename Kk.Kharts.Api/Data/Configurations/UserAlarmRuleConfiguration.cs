using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations
{

    public class UserAlarmRuleConfiguration : IEntityTypeConfiguration<UserAlarmRule>
    {
        public void Configure(EntityTypeBuilder<UserAlarmRule> builder)
        {
            builder.ToTable("user_alarm_rules"); // Opcional: define o nome da tabela no DB

            // Define a chave primária composta
            builder.HasKey(uar => new { uar.UserId, uar.AlarmRuleId });

            // Configura o relacionamento de User para UserAlarmRule
            builder.HasOne(uar => uar.User)
                   .WithMany(u => u.UserAlarmRules)
                   .HasForeignKey(uar => uar.UserId)
                   .OnDelete(DeleteBehavior.NoAction); // <-- Recomendado para evitar conflitos de cascata

            // Configura o relacionamento de AlarmRule para UserAlarmRule
            builder.HasOne(uar => uar.AlarmRule)
                   .WithMany(ar => ar.UserAlarmRules)
                   .HasForeignKey(uar => uar.AlarmRuleId)
                   .OnDelete(DeleteBehavior.NoAction); // <-- Recomendado para evitar conflitos de cascata
        }
    }
}
