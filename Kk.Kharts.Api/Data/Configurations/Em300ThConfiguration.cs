using Kk.Kharts.Api.Utility;
using Kk.Kharts.Shared.Entities.Em300;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Kk.Kharts.Api.Data.Configurations;

public class Em300ThConfiguration : IEntityTypeConfiguration<Em300Th>
{
    public void Configure(EntityTypeBuilder<Em300Th> builder)
    {
        builder.ToTable("em300_th");

        // Definindo a chave primária composta
        builder.HasKey(e => new { e.Timestamp, e.DevEui });

        // Definindo as colunas e a ordem delas
        builder.Property(p => p.Timestamp).HasColumnName("timestamp").HasColumnOrder(0);
        builder.Property(p => p.DevEui).HasColumnName("dev_eui").HasColumnOrder(1).HasMaxLength(16);
        builder.Property(p => p.DeviceId).HasColumnName("device_id").HasColumnOrder(2);
        builder.Property(p => p.Temperature).HasColumnName("temperature").HasColumnOrder(3);
        builder.Property(p => p.Humidity).HasColumnName("humidite").HasColumnOrder(4);
        builder.Property(p => p.Battery).HasColumnName("batterie").HasColumnOrder(5);

        builder.HasOne(e => e.Device)
                  .WithMany(d => d.Em300ths)
                  .HasForeignKey(e => e.DeviceId)
                  .IsRequired(false) // Permitir que Device seja opcional
                  .OnDelete(DeleteBehavior.Cascade);
    }
}