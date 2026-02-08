using KK.UG6x.StoreAndForward.Domain.Interfaces;
using KK.UG6x.StoreAndForward.Infrastructure;
using KK.UG6x.StoreAndForward.Persistence;
using KK.UG6x.StoreAndForward.Security;
using KK.UG6x.StoreAndForward.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace KK.UG6x.StoreAndForward.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, AppEnvironmentPaths paths)
    {
        services.AddHttpClient();
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite($"Data Source={paths.DbPath}"));

        services.AddSingleton<DashboardStateService>();
        services.AddSingleton<AppSettingsService>();
        services.AddSingleton<ILocalStore, LocalStore>();
        services.AddSingleton<IGatewayDiscoveryService, GatewayDiscoveryService>();
        services.AddSingleton<IUG65Client, UG65Client>();
        services.AddSingleton<IKhartsApiClient, KhartsApiClient>();

        services.AddHostedService<Worker>();

        services.AddAuthentication("Bearer")
            .AddScheme<AuthenticationSchemeOptions, TokenAuthHandler>("Bearer", null);
        services.AddAuthorization();

        return services;
    }
}
