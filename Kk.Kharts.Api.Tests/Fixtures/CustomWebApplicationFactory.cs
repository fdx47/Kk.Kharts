using Kk.Kharts.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Kk.Kharts.Api.Tests.Fixtures;

/// <summary>
/// Factory personnalisée pour les tests d'intégration.
/// Configure l'application avec une base de données en mémoire
/// et désactive les services externes (Telegram, etc.).
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Schema"] = "test_schema"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Supprimer le DbContext réel et ses dépendances
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            // S'assurer que IHttpContextAccessor est disponible
            services.TryAddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor>(
                new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object);

            // Ajouter le DbContext en mémoire
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("KkKhartsTestDb");
            });
        });
    }
}
