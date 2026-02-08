using Kk.Kharts.Shared.Entities;
using Kk.Kharts.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {      
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.Id);

            builder.Property(p => p.Id)
              .HasColumnName("id")
              .HasColumnOrder(0);

            builder.Property(u => u.Nom)
                   .HasColumnName("nom")
                   .HasColumnOrder(1)
                   .IsRequired();

            builder.Property(u => u.Email)
                    .HasColumnName("email")
                   .HasColumnOrder(2)
                   .IsRequired()
                   .HasMaxLength(255); // opcional, recomendado para e-mail

            builder.Property(u => u.Role)
                .HasColumnName("role")
                   .HasColumnOrder(3)
                    .IsRequired();

            builder.Property(u => u.CompanyId)
                     .HasColumnName("company_id")
                     .HasColumnOrder(4);

            builder.Property(u => u.AccessLevel)
                    .HasColumnName("access_level")
                    .HasColumnOrder(5)
                  .HasConversion<int>(); // enum convertido para int

            builder.Property(u => u.Password)
                .HasColumnName("password")
                   .HasColumnOrder(6)
                   .IsRequired();

            builder.Property(u => u.HeaderName)
                    .HasColumnName("header_name")
                   .HasColumnOrder(7)
                   .HasDefaultValue("3ctec");

            builder.Property(u => u.HeaderValue)
               .HasColumnName("header_value")
                   .HasColumnOrder(8)
                   .HasDefaultValue("demo");

            builder.Property(u => u.LastUserAgent)
                   .HasColumnName("last_user_agent")
                   .HasColumnOrder(9)
                   .HasMaxLength(512);

            builder.Property(u => u.LastIpAddress)
                   .HasColumnName("last_ip_address")
                   .HasColumnOrder(10)
                   .HasMaxLength(45); // IPv6 pode ter até 45 caracteres

            builder.Property(u => u.RefreshToken)
                .HasColumnName("refresh_token")
                .HasColumnOrder(11)
                   .HasMaxLength(512);

            builder.Property(u => u.RefreshTokenExpiryTime)
                .HasColumnName("refresh_token_expiry_time").
                HasColumnOrder(12)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(u => u.SignupDate)
                     .HasColumnName("signup_date)\r\n")
                     .HasColumnOrder(13)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Telegram Integration
            builder.Property(u => u.TelegramUserId)
                   .HasColumnName("telegram_user_id")
                   .HasColumnOrder(14);

            builder.Property(u => u.TelegramUsername)
                   .HasColumnName("telegram_username")
                   .HasColumnOrder(15)
                   .HasMaxLength(100);

            builder.Property(u => u.TelegramLinkedAt)
                   .HasColumnName("telegram_linked_at")
                   .HasColumnOrder(16);

            builder.Property(u => u.NotificationPreference)
                   .HasColumnName("notification_preference")
                   .HasColumnOrder(17)
                   .HasConversion<int>();

            builder.OwnsOne(u => u.Pushover, navigationBuilder =>
            {
                navigationBuilder.ToTable("user_pushover_settings");
                navigationBuilder.WithOwner().HasForeignKey("user_id");

                navigationBuilder.Property(p => p.AppToken)
                    .HasColumnName("app_token")
                    .HasMaxLength(64);

                navigationBuilder.Property(p => p.UserKey)
                    .HasColumnName("user_key")
                    .HasMaxLength(64);

                navigationBuilder.Property(p => p.Sound)
                    .HasColumnName("sound")
                    .HasMaxLength(32);

                navigationBuilder.Property(p => p.Device)
                    .HasColumnName("device")
                    .HasMaxLength(32);

                navigationBuilder.Property(p => p.Title)
                    .HasColumnName("title")
                    .HasMaxLength(120);

                navigationBuilder.Property(p => p.MessageTemplate)
                    .HasColumnName("message_template")
                    .HasMaxLength(512);

                navigationBuilder.Property(p => p.Priority)
                    .HasColumnName("priority");

                navigationBuilder.Property(p => p.RetrySeconds)
                    .HasColumnName("retry_seconds");

                navigationBuilder.Property(p => p.ExpireSeconds)
                    .HasColumnName("expire_seconds");
            });

            builder.Navigation(u => u.Pushover)
                   .IsRequired(false);

            builder.HasIndex(u => u.TelegramUserId)
                   .IsUnique()
                   .HasFilter("[telegram_user_id] IS NOT NULL");

            // Relacionamento com Company
            builder.HasOne(u => u.Company)
                   .WithMany(s => s.Users)
                   .HasForeignKey(u => u.CompanyId)
                   .IsRequired();
        }
    }
}
