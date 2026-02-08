using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations;

public class VpnProfileConfiguration : IEntityTypeConfiguration<VpnProfile>
{
    public void Configure(EntityTypeBuilder<VpnProfile> builder)
    {
        builder.ToTable("vpn_profiles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(v => v.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.CommonName)
            .HasColumnName("common_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.VpnIp)
            .HasColumnName("vpn_ip")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Notes)
            .HasColumnName("notes")
            .HasMaxLength(500);

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(v => v.OvpnContent)
            .HasColumnName("ovpn_content")
            .HasColumnType("nvarchar(max)");

        builder.Property(v => v.OvpnFileName)
            .HasColumnName("ovpn_file_name")
            .HasMaxLength(200);

        builder.Property(v => v.AssignedUserId)
            .HasColumnName("assigned_user_id");

        builder.Property(v => v.AssignedCompanyId)
            .HasColumnName("assigned_company_id");

        builder.Property(v => v.InstallationLocation)
            .HasColumnName("installation_location")
            .HasMaxLength(500);

        builder.Property(v => v.AssignedAt)
            .HasColumnName("assigned_at");

        builder.Property(v => v.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(v => v.AssignedUser)
            .WithMany()
            .HasForeignKey(v => v.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(v => v.AssignedCompany)
            .WithMany()
            .HasForeignKey(v => v.AssignedCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.CommonName)
            .IsUnique();

        builder.HasIndex(v => v.VpnIp)
            .IsUnique();

        builder.HasIndex(v => v.AssignedUserId);
        builder.HasIndex(v => v.AssignedCompanyId);
    }
}
