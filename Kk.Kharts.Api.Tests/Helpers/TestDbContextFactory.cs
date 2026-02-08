using Kk.Kharts.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Kk.Kharts.Api.Tests.Helpers;

/// <summary>
/// Factory pour créer des instances de DbContext en mémoire pour les tests.
/// Isolation garantie entre les tests via des noms de base de données uniques.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Crée un DbContext avec la base de données en mémoire.
    /// Chaque appel crée une nouvelle base de données isolée.
    /// </summary>
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Configuration mock avec le schéma requis
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Schema"] = "test_schema"
            })
            .Build();

        // HttpContextAccessor mock
        var httpContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns((Microsoft.AspNetCore.Http.HttpContext?)null);

        var context = new AppDbContext(options, configuration, httpContextAccessor.Object);
        context.Database.EnsureCreated();
        return context;
    }
}
