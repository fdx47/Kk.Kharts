using Kk.Kharts.Api.Utility;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations
{
    public class DeviceModelConfiguration : IEntityTypeConfiguration<DeviceModel>
    {


        public void Configure(EntityTypeBuilder<DeviceModel> builder)
        {

            builder.ToTable("device_model");
            //builder.ToTable("device_model", _schema);

            builder.HasKey(e => e.ModelId);

            // Definindo que o Id é auto-incremento
            builder.Property(p => p.ModelId)
                   .HasColumnOrder(1)
                   .ValueGeneratedOnAdd() // Valor gerado ao adicionar
                   .UseIdentityColumn(); // Auto-incremento no SQL Server

            builder.Property(p => p.Model)
                .HasColumnName("model")
                .HasColumnOrder(2)
                .HasMaxLength(300);

            builder.Property(p => p.Description)
                .HasColumnName("description")
                .HasColumnOrder(3)
                .HasMaxLength(500);


            builder.HasData(
                      new DeviceModel { ModelId = 1, Model = "EM300-CL", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 2, Model = "EM300-DI", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 3, Model = "EM300-DI_Hall", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 4, Model = "EM300-MCS", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 5, Model = "EM300-MLD", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 6, Model = "EM300-SLD", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 7, Model = "EM300-TH", Description = "Temperature & Humidity Sensor" },
                      new DeviceModel { ModelId = 8, Model = "EM310-TILT", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 9, Model = "EM310-UDL", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 10, Model = "EM320-TH", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 11, Model = "EM320-TILT", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 12, Model = "EM300-Reserved9", Description = "Reserved" },
                      new DeviceModel { ModelId = 13, Model = "EM300-Reserved8", Description = "Reserved" },
                      new DeviceModel { ModelId = 14, Model = "EM300-Reserved7", Description = "Reserved" },
                      new DeviceModel { ModelId = 15, Model = "EM300-Reserved6", Description = "Reserved" },
                      new DeviceModel { ModelId = 16, Model = "EM300-Reserved5", Description = "Reserved" },
                      new DeviceModel { ModelId = 17, Model = "EM300-Reserved4", Description = "Reserved" },
                      new DeviceModel { ModelId = 18, Model = "EM300-Reserved3", Description = "Reserved" },
                      new DeviceModel { ModelId = 19, Model = "EM300-Reserved2", Description = "Reserved" },
                      new DeviceModel { ModelId = 20, Model = "EM300-Reserved1", Description = "Reserved" },

                      new DeviceModel { ModelId = 21, Model = "EM500-CO2", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 22, Model = "EM500-LGT", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 23, Model = "EM500-PP", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 24, Model = "EM500-PT100", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 25, Model = "EM500-SMTC", Description = "Soil Moisture, Temperature and Electrical Conductivity Sensor" },
                      new DeviceModel { ModelId = 26, Model = "EM500-SWL", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 27, Model = "EM500-UDL", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 28, Model = "EM500-Reserved13", Description = "Reserved" },
                      new DeviceModel { ModelId = 29, Model = "EM500-Reserved12", Description = "Reserved" },
                      new DeviceModel { ModelId = 30, Model = "EM500-Reserved11", Description = "Reserved" },
                      new DeviceModel { ModelId = 31, Model = "EM500-Reserved10", Description = "Reserved" },
                      new DeviceModel { ModelId = 32, Model = "EM500-Reserved9", Description = "Reserved" },
                      new DeviceModel { ModelId = 33, Model = "EM500-Reserved8", Description = "Reserved" },
                      new DeviceModel { ModelId = 34, Model = "EM500-Reserved7", Description = "Reserved" },
                      new DeviceModel { ModelId = 35, Model = "EM500-Reserved6", Description = "Reserved" },
                      new DeviceModel { ModelId = 36, Model = "EM500-Reserved5", Description = "Reserved" },
                      new DeviceModel { ModelId = 37, Model = "EM500-Reserved4", Description = "Reserved" },
                      new DeviceModel { ModelId = 38, Model = "EM500-Reserved3", Description = "Reserved" },
                      new DeviceModel { ModelId = 39, Model = "EM500-Reserved2", Description = "Reserved" },
                      new DeviceModel { ModelId = 40, Model = "EM500-Reserved1", Description = "Reserved" },

                      new DeviceModel { ModelId = 41, Model = "UC100", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 42, Model = "UC11-N1", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 43, Model = "UC11-T1", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 44, Model = "UC11XX", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 45, Model = "UC300", Description = "UC300 Iot Controller" },
                      new DeviceModel { ModelId = 46, Model = "UC3XXX", Description = "xxxxxxx" },
                      new DeviceModel { ModelId = 47, Model = "UC502", Description = "Multi-Interface Controller" },
                      new DeviceModel { ModelId = 48, Model = "UC51X", Description = "Reserved" },
                      new DeviceModel { ModelId = 49, Model = "UC-Series-Reserved12", Description = "Reserved" },
                      new DeviceModel { ModelId = 50, Model = "UC-Series-Reserved11", Description = "Reserved" },
                      new DeviceModel { ModelId = 51, Model = "UC-Series-Reserved10", Description = "Reserved" },
                      new DeviceModel { ModelId = 52, Model = "UC-Series-Reserved9", Description = "Reserved" },
                      new DeviceModel { ModelId = 53, Model = "UC-Series-Reserved8", Description = "Reserved" },
                      new DeviceModel { ModelId = 54, Model = "UC-Series-Reserved7", Description = "Reserved" },
                      new DeviceModel { ModelId = 55, Model = "UC-Series-Reserved6", Description = "Reserved" },
                      new DeviceModel { ModelId = 56, Model = "UC-Series-Reserved5", Description = "Reserved" },
                      new DeviceModel { ModelId = 57, Model = "UC-Series-Reserved4", Description = "Reserved" },
                      new DeviceModel { ModelId = 58, Model = "UC-Series-Reserved3", Description = "Reserved" },
                      new DeviceModel { ModelId = 59, Model = "UC-Series-Reserved2", Description = "Reserved" },
                      new DeviceModel { ModelId = 60, Model = "UC-Series-Reserved1", Description = "Reserved" },
                      /////////////////////////////////////// Custom ///////////////////////////////////////
                      new DeviceModel { ModelId = 61, Model = "U502", Description = "Modbus" },
                      new DeviceModel { ModelId = 62, Model = "U502_Wet150_x2_Sensors", Description = "U502 Wet150 device with 2 sensors" },
                      new DeviceModel { ModelId = 63, Model = "U502_Wet150_x3_Sensors", Description = "U502 Wet150 device with 3 sensors" },
                      new DeviceModel { ModelId = 64, Model = "U502_Wet150_x4_Sensors", Description = "U502 Wet150 device with 4 sensors" }
                      );

        }
    }
}
