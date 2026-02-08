using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations;

public sealed class TemporaryAccessTokenConfiguration : IEntityTypeConfiguration<TemporaryAccessToken>
{
    private const string TableName = "temporary_access_tokens";

    public void Configure(EntityTypeBuilder<TemporaryAccessToken> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(t => t.TokenPrefix)
            .HasColumnName("token_prefix")
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(t => t.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        builder.Property(t => t.ExpiresAtUtc)
            .HasColumnName("expires_at_utc")
            .IsRequired();

        builder.Property(t => t.IssuedByUserId)
            .HasColumnName("issued_by_user_id");

        builder.Property(t => t.IssuedByEmail)
            .HasColumnName("issued_by_email")
            .HasMaxLength(320);

        builder.Property(t => t.IssuedByTelegramUserId)
            .HasColumnName("issued_by_telegram_user_id");

        builder.Property(t => t.UsageCount)
            .HasColumnName("usage_count")
            .HasDefaultValue(0);

        builder.Property(t => t.LastUsedAtUtc)
            .HasColumnName("last_used_at_utc");

        builder.Property(t => t.ConsumedByUserId)
            .HasColumnName("consumed_by_user_id");

        builder.Property(t => t.ConsumedByEmail)
            .HasColumnName("consumed_by_email")
            .HasMaxLength(320);

        builder.Property(t => t.Revoked)
            .HasColumnName("revoked")
            .HasDefaultValue(false);

        builder.HasIndex(t => t.TokenPrefix)
            .HasDatabaseName("IX_temporary_access_tokens_prefix");

        builder.HasIndex(t => t.ExpiresAtUtc)
            .HasDatabaseName("IX_temporary_access_tokens_expiration");

        builder.HasIndex(t => t.TokenHash)
            .HasDatabaseName("UX_temporary_access_tokens_hash")
            .IsUnique();
    }
}
