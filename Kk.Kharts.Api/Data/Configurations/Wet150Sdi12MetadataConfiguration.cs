using Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations
{
    public class Wet150Sdi12MetadataConfiguration : IEntityTypeConfiguration<Wet150Sdi12Metadata>
    {
        public void Configure(EntityTypeBuilder<Wet150Sdi12Metadata> builder)
        {
            builder.ToTable("wet150_sdi12_metadatas"); // Nome da tabela

            builder.HasKey(x => x.Id); // Chave primária

            builder.Property(x => x.DevEui)
                .IsRequired()
                .HasMaxLength(16);

            builder.Property(x => x.Sdi12Index)
                .IsRequired();

            builder.Property(x => x.Sdi12Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Sdi12InstallationLocation)
                .HasMaxLength(200);

            builder.HasOne(x => x.Device)
                .WithMany() // ou .WithOne() se for 1:1
                .HasForeignKey(x => x.DevEui)
                .HasPrincipalKey(d => d.DevEui) // Usa DevEui como chave de navegação
                .OnDelete(DeleteBehavior.Cascade); // ou Restrict
        }
    }
}
