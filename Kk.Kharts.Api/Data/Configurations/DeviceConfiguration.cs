using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations
{
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {

            builder.ToTable("devices");

            builder.Property(p => p.Id)
                   .HasColumnOrder(1)
                   .ValueGeneratedOnAdd() // Valor gerado automaticamente ao adicionar
                   .UseIdentityColumn(); // Auto-incremento no SQL Server

            // Relacionamento com o Modelo de Navegação
            builder.HasOne(d => d.ModeloNavegacao)
                   .WithMany() // Não há coleção em DeviceModel
                   .HasForeignKey(d => d.DeviceModel)
                   .HasConstraintName("FK_devices_devices_models_DeviceModelId")
                   .OnDelete(DeleteBehavior.Cascade);

            // Configuração do relacionamento com Societe
            builder.HasOne(d => d.Company)  // Device tem um relacionamento com Societe
                   .WithMany()  // Não há necessidade de coleções de dispositivos na Societe
                   .HasForeignKey(d => d.CompanyId)  // SocieteId é a chave estrangeira
                   .OnDelete(DeleteBehavior.SetNull);  // Se Societe for excluída, SocieteId será null


            builder.Property(p => p.DevEui)
                .HasColumnName("dev_eui")
                .HasColumnOrder(2)
                .HasMaxLength(16);

            builder.Property(p => p.Name)
                .HasColumnName("name")
                .HasColumnOrder(3)
                .HasMaxLength(16);

            builder.Property(p => p.Description)
                .HasColumnName("description")
                .HasColumnOrder(4)
                .HasMaxLength(40);

            builder.Property(p => p.CompanyId)
               .HasColumnName("societe_id")
               .HasColumnOrder(5);

            builder.Property(p => p.Battery)
               .HasColumnName("battery")
               .HasColumnOrder(6);

            builder.Property(p => p.LastSendAt)
               .HasColumnName("last_send_at")
               .HasColumnOrder(7)
               .HasMaxLength(40);

            builder.Property(p => p.LastSeenAt)
                .HasColumnName("last_seen_at")
                .HasColumnOrder(8)
                .HasMaxLength(32);



            builder.Property(p => p.DeviceModel)
               .HasColumnName("model_id")
               .HasColumnOrder(10);

            builder.Property(p => p.ActiveInKropKontrol)
                .HasColumnName("active_in_kropkontrol")
                .HasColumnOrder(11);

            builder.Property(p => p.HasCommunicationAlarm)
               .HasColumnName("has_comm_alarm")
               .HasColumnOrder(12);

            builder.Property(p => p.InstallationLocation)
               .HasColumnName("Installation_location")
               .HasColumnOrder(13)
               .HasMaxLength(50)
               .IsRequired(false);


            builder.HasMany(d => d.AlarmRules)
               .WithOne(ar => ar.Device)
               .HasForeignKey(ar => ar.DeviceId)
               .OnDelete(DeleteBehavior.Cascade);

            // Inserindo dados iniciais
            builder.HasData(
                new Device
                {
                    Id = 16,
                    DevEui = "24E124136E311882",
                    Name = "EM300_TH_006",
                    Description = "Temp & Hr - 006",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    ActiveInKropKontrol = true,
                    DeviceModel = 7,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },
                new Device
                {
                    Id = 12,
                    DevEui = "24E124136E317494",
                    Name = "EM300_TH_002",
                    Description = "Temp & Hr - 002",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    ActiveInKropKontrol = true,
                    DeviceModel = 7,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),

                },
                new Device
                {
                    Id = 14,
                    DevEui = "24E124136E311302",
                    Name = "EM300_TH_004",
                    Description = "Temp & Hr - 004",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    DeviceModel = 7,
                    ActiveInKropKontrol = true,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },
                new Device
                {
                    Id = 13,
                    DevEui = "24E124136E317560",
                    Name = "EM300_TH_003",
                    Description = "Temp & Hr - 003",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    DeviceModel = 7,
                    ActiveInKropKontrol = true,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },
                new Device
                {
                    Id = 11,
                    DevEui = "24E124136E318864",
                    Name = "EM300_TH_001",
                    Description = "Temp & Hr - 001",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    DeviceModel = 7,
                    ActiveInKropKontrol = true,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },
                new Device
                {
                    Id = 15,
                    DevEui = "24E124136E316874",
                    Name = "EM300_TH_005",
                    Description = "Temp & Hr - 005",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    DeviceModel = 7,
                    ActiveInKropKontrol = true,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },
                new Device
                {
                    Id = 6,
                    DevEui = "24E124454E353679",
                    Name = "UC502_006",
                    Description = "WET150 - 006",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    DeviceModel = 47,
                    ActiveInKropKontrol = true,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },
                new Device
                {
                    Id = 8,
                    DevEui = "24E124454E353717",
                    Name = "UC502_008",
                    Description = "WET150 - 008",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    DeviceModel = 47,
                    ActiveInKropKontrol = true,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },
                new Device
                {
                    Id = 4,
                    DevEui = "24E124454E042988",
                    Name = "UC502_004",
                    Description = "WET150 - 004",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    DeviceModel = 47,
                    ActiveInKropKontrol = true,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },
                new Device
                {
                    Id = 2,
                    DevEui = "24E124454E353580",
                    Name = "UC502_002",
                    Description = "WET150 - 002",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    DeviceModel = 47,
                    ActiveInKropKontrol = true,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },
                new Device
                {
                    Id = 1,
                    DevEui = "24E124454E353385",
                    Name = "UC502_001",
                    Description = "WET150 - 001",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    DeviceModel = 47,
                    ActiveInKropKontrol = true,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },
                new Device
                {
                    Id = 7,
                    DevEui = "24E124454E048465",
                    Name = "UC502_007",
                    Description = "WET150 - 007",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    DeviceModel = 47,
                    ActiveInKropKontrol = true,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },
                new Device
                {
                    Id = 9,
                    DevEui = "24E124454E353777",
                    Name = "UC502_009",
                    Description = "WET150 - 009",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    DeviceModel = 47,
                    ActiveInKropKontrol = true,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },
                new Device
                {
                    Id = 3,
                    DevEui = "24E124454E353875",
                    Name = "UC502_003",
                    Description = "WET150 - 003",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    DeviceModel = 47,
                    ActiveInKropKontrol = true,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },
                new Device
                {
                    Id = 5,
                    DevEui = "24E124454E353844",
                    Name = "UC502_005",
                    Description = "WET150 - 005",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),                  
                    DeviceModel = 47,
                    ActiveInKropKontrol = true,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },
                new Device
                {
                    Id = 10,
                    DevEui = "24E124454E352976",
                    Name = "UC502_010",
                    Description = "Invenio Tomate",
                    Battery = 69.69f,
                    LastSendAt = "2017 - 10 - 25 00:00:00",
                    LastSeenAt = new DateTime(2017, 10, 25),
                    DeviceModel = 47,
                    ActiveInKropKontrol = true,
                    CompanyId = 3,
                    Seller = "3CTEC",
                    InstallationDate = new DateTime(2024, 10, 27),
                },

                 ///////////////////////////////////
                 new Device
                 {
                     Id = 17,
                     DevEui = "24E124454E045483",
                     Name = "UC502_045483",
                     Description = "Demo Pozzobon Modbus - Solarimètre",
                     Battery = 69.69f,
                     LastSendAt = "2017 - 10 - 25 00:00:00",
                     LastSeenAt = new DateTime(2017, 10, 25),
                     DeviceModel = 61,
                     ActiveInKropKontrol = true,
                     CompanyId = 2,
                     Seller = "3CTEC",
                     InstallationDate = new DateTime(2024, 05, 12),
                 },
                 new Device
                 {
                     Id = 18,
                     DevEui = "24E124454E353793",
                     Name = "UC502_353793",
                     Description = "Demo Pozzobon - Wet150",
                     Battery = 69.69f,
                     LastSendAt = "2017 - 10 - 25 00:00:00",
                     LastSeenAt = new DateTime(2017, 10, 25),
                     DeviceModel = 47,
                     ActiveInKropKontrol = true,
                     CompanyId = 4,
                     Seller = "3CTEC",
                     InstallationDate = new DateTime(2025, 05, 12),
                 }
            );
        }
    }
}

