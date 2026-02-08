using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Data.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("companies");

            // Relação com filiais
            builder.HasMany(s => s.Subsidiaries)
                   .WithOne(s => s.ParentCompany)
                   .HasForeignKey(s => s.ParentCompanyId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relação com usuários
            builder.HasMany(s => s.Users)
                   .WithOne(u => u.Company)
                   .HasForeignKey(u => u.CompanyId)
                   .IsRequired();

            builder.Property(p => p.Id)
               .HasColumnName("id")
               .HasColumnOrder(0);

            builder.Property(p => p.Name)
               .HasColumnName("name")
               .HasColumnOrder(1)
               .HasMaxLength(250)
               .IsRequired(); 

            builder.Property(p => p.ParentCompanyId)
              .HasColumnName("parent_company_id")
              .HasColumnOrder(2);

            builder.Property(u => u.HeaderNameApiKey)
                   .HasColumnName("header_name_apikey")
                  .HasColumnOrder(3)
                  .HasDefaultValue("3ctec")
                  .HasMaxLength(50); ;

            builder.Property(u => u.HeaderValueApiKey)
               .HasColumnName("header_value_apikey")
                   .HasColumnOrder(4)
                   .HasDefaultValue("DefautApikeyDemoKk2025")
                   .HasMaxLength(57);

            builder.HasData(
                new Company { Id = 1, Name = "3CTEC" },
                new Company { Id = 2, Name = "Stratberries" },
                new Company { Id = 3, Name = "Invenio" },
                new Company { Id = 4, Name = "Pozzobon" },
                new Company { Id = 5, Name = "Baudas" }
                //new Societe { Id = 6, Nom = "3CTEC Biz 1", SocieteMereId = 1 },   // Filial de 3ctec
                //new Societe { Id = 7, Nom = "3CTEC Biz 2", SocieteMereId = 1 },   // Filial de 3ctec

            );
        }
    }
}
