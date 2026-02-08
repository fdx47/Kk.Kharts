using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Kk.Kharts.Api.Data.Configurations;

public class DeviceParameterConfiguration : IEntityTypeConfiguration<DeviceParameter>
{
    public void Configure(EntityTypeBuilder<DeviceParameter> builder)
    {
        // Nome da tabela
        builder.ToTable("devices_parameters");

        // Chave primária
        builder.HasKey(p => p.Id);

        // Relacionamento 1:1 com Device
        builder.HasOne(p => p.Device)
               .WithOne(d => d.Em300DiParameters)
               .HasForeignKey<DeviceParameter>(p => p.DeviceId)
               .OnDelete(DeleteBehavior.Cascade); // ou Restrict, se quiser proteger

        // Propriedades
        builder.Property(p => p.DeviceId)
               .HasColumnName("device_id")
               .IsRequired();

        builder.Property(p => p.WaterConversion)
               .HasColumnName("water_conversion")
               .HasColumnType("float");

        builder.Property(p => p.PulseConversion)
               .HasColumnName("pulse_conversion")
               .HasColumnType("float");

        builder.Property(p => p.AlarmThreshold)
               .HasColumnName("alarm_threshold")
               .HasColumnType("float");
    }
}