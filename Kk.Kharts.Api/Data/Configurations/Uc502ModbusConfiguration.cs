using Kk.Kharts.Shared.Entities.UC502;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations
{

    public class Uc502ModbusConfiguration : IEntityTypeConfiguration<Uc502Modbus>
    {

        public void Configure(EntityTypeBuilder<Uc502Modbus> builder)
        {
            builder.ToTable("uc502_modbus");

            builder.HasKey(e => new { e.Timestamp, e.DevEui });

            builder.Property(p => p.Timestamp).HasColumnName("timestamp").HasColumnOrder(0);
            builder.Property(p => p.DevEui).HasColumnName("dev_eui").HasColumnOrder(1).HasMaxLength(16);
            builder.Property(p => p.DeviceId).HasColumnName("device_id").HasColumnOrder(2);

            builder.Property(p => p.ModbusChannel1).HasColumnName("modbus_chn_1").HasColumnOrder(3);
            builder.Property(p => p.ModbusChannel2).HasColumnName("modbus_chn_2").HasColumnOrder(4);
            builder.Property(p => p.ModbusChannel3).HasColumnName("modbus_chn_3").HasColumnOrder(5);
            builder.Property(p => p.ModbusChannel4).HasColumnName("modbus_chn_4").HasColumnOrder(6);
            builder.Property(p => p.ModbusChannel5).HasColumnName("modbus_chn_5").HasColumnOrder(7);
            builder.Property(p => p.ModbusChannel6).HasColumnName("modbus_chn_6").HasColumnOrder(8);

            //builder.Property(p => p.ModbusChannel7).HasColumnName("modbus_chn_7").HasColumnOrder(9);
            //builder.Property(p => p.ModbusChannel8).HasColumnName("modbus_chn_8").HasColumnOrder(10);
            //builder.Property(p => p.ModbusChannel9).HasColumnName("modbus_chn_9").HasColumnOrder(11);
            //builder.Property(p => p.ModbusChannel10).HasColumnName("modbus_chn_10").HasColumnOrder(12);
            //builder.Property(p => p.ModbusChannel11).HasColumnName("modbus_chn_11").HasColumnOrder(13);
            //builder.Property(p => p.ModbusChannel12).HasColumnName("modbus_ch_12").HasColumnOrder(14);
            //builder.Property(p => p.ModbusChannel13).HasColumnName("modbus_chn_13").HasColumnOrder(15);
            //builder.Property(p => p.ModbusChannel14).HasColumnName("modbus_chn_14").HasColumnOrder(16);
            //builder.Property(p => p.ModbusChannel15).HasColumnName("modbus_chn_15").HasColumnOrder(17);
            //builder.Property(p => p.ModbusChannel16).HasColumnName("modbus_chn_16").HasColumnOrder(18);

            builder.Property(p => p.Battery).HasColumnName("batterie").HasColumnOrder(18);

            builder.HasOne(e => e.Device)
                 .WithMany(d => d.Uc502Modbuss)
                 .HasForeignKey(e => e.DeviceId)
                 .IsRequired(false) // Permitir que Device seja opcional
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}