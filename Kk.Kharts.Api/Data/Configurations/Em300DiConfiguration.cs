using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Kk.Kharts.Api.Data.Configurations;

public class Em300DiConfiguration : IEntityTypeConfiguration<Em300Di>
{
    public void Configure(EntityTypeBuilder<Em300Di> builder)
    {
        builder.ToTable("em300_di");

        // Definindo a chave primária composta
        builder.HasKey(e => new { e.Timestamp, e.DevEui });

        // Definindo as colunas e a ordem delas
        builder.Property(p => p.Timestamp).HasColumnName("timestamp").HasColumnOrder(0);
        builder.Property(p => p.DevEui).HasColumnName("dev_eui").HasColumnOrder(1).HasMaxLength(16);
        builder.Property(p => p.DeviceId).HasColumnName("device_id").HasColumnOrder(2);
        builder.Property(p => p.Temperature).HasColumnName("temperature").HasColumnOrder(3);
        builder.Property(p => p.Humidity).HasColumnName("humidite").HasColumnOrder(4);
        builder.Property(p => p.Water).HasColumnName("water").HasColumnOrder(5);
        builder.Property(p => p.Battery).HasColumnName("batterie").HasColumnOrder(6);

        builder.HasOne(e => e.Device)
                  .WithMany(d => d.Em300dis)
                  .HasForeignKey(e => e.DeviceId)
                  .IsRequired(false) // Permitir que Device seja opcional
                  .OnDelete(DeleteBehavior.Cascade);
    }
}