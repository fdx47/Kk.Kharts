using Kk.Kharts.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=db19330.public.databaseasp.net; Database=db19330; User Id=db19330; Password=4Na+Y=8s6e#W; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;");

        // Charger la configuration depuis appsettings.json (facultatif)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())  
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        // Créer un logger factice pour usage design-time
        using var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
        var logger = loggerFactory.CreateLogger<AppDbContext>();

        // cria um HttpContextAccessor “vazio” só pra satisfazer o construtor (Design-time only)
        var httpContextAccessor = new HttpContextAccessor();

        return new AppDbContext(optionsBuilder.Options, configuration, /*logger,*/ httpContextAccessor);
    }
}



//Design‐time factory (para Add-Migration)
//No caso das migrations, o EF Core não passa pelo DI normal, e por isso não encontra o IHttpContextAccessor nem a connection string.

//Em runtime, o AppDbContext é resolvido pelo DI normal (já com o IHttpContextAccessor injetado).

//Em design‐time (ou seja, quando você roda Add-Migration), o EF vai usar a fábrica acima para criar o contexto e conseguir aplicar todas as suas IEntityTypeConfiguration<>, incluindo a sua chave composta HasKey(...).