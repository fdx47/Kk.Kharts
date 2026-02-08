using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kk.Kharts.Api.Data.Configurations
{
    public class SoilParameterConfiguration : IEntityTypeConfiguration<SoilParameter>
    {
        public void Configure(EntityTypeBuilder<SoilParameter> builder)
        {
            builder.ToTable("soil_parameters");

            builder.HasKey(sp => sp.Id);

            builder.Property(sp => sp.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(sp => sp.A0).IsRequired();
            builder.Property(sp => sp.A1).IsRequired();
            builder.Property(sp => sp.Epsilon0).IsRequired();

            // Seed de dados
            builder.HasData(
                new SoilParameter { Id = 1, Name = "Minéral", A0 = 1.6f, A1 = 8.4f, Epsilon0 = -4.0f },
                new SoilParameter { Id = 2, Name = "Organique", A0 = 1.3f, A1 = 7.7f, Epsilon0 = 5.5f },
                new SoilParameter { Id = 3, Name = "Mélange de tourbe", A0 = 1.16f, A1 = 7.09f, Epsilon0 = 1.8f },
                new SoilParameter { Id = 4, Name = "Fibre de coco", A0 = 1.16f, A1 = 7.41f, Epsilon0 = 0.0f },
                new SoilParameter { Id = 5, Name = "Laine minérale", A0 = 1.04f, A1 = 7.58f, Epsilon0 = -0.3f },
                new SoilParameter { Id = 6, Name = "Perlite", A0 = 1.06f, A1 = 6.53f, Epsilon0 = 4.1f },
                new SoilParameter { Id = 7, Name = "Custom 1", A0 = 1.06f, A1 = 6.53f, Epsilon0 = 4.1f },
                new SoilParameter { Id = 8, Name = "Custom 2", A0 = 1.06f, A1 = 6.53f, Epsilon0 = 4.1f },
                new SoilParameter { Id = 9, Name = "Custom 3", A0 = 1.06f, A1 = 6.53f, Epsilon0 = 4.1f }
            );
        }
    }
}
