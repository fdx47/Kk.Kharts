using KK.UG6x.StoreAndForward.Extensions;
using KK.UG6x.StoreAndForward.Endpoints;
using KK.UG6x.StoreAndForward.Infrastructure;
using KK.UG6x.StoreAndForward.Logging;
using KK.UG6x.StoreAndForward.Persistence;
using KK.UG6x.StoreAndForward.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Net;
using System.Net.Sockets;
using System.Threading;

var paths = AppEnvironment.Initialize();

const string mutexName = "Global\\KK_UG6x_StoreAndForward";
using var instanceMutex = new Mutex(initiallyOwned: true, mutexName, out var createdNew);
if (!createdNew)
{
    Console.WriteLine("[Bootstrap] Une autre instance du service est déjà en cours d'exécution. Fermeture...");
    return;
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Host
    .UseWindowsService();

builder.Services.AddApplicationServices(paths);

builder.Host.UseSerilog((context, services, configuration) =>
{
    var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app-.log");
    var errorPath = Path.Combine(AppContext.BaseDirectory, "logs", "errors-.log");

    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext();

    if (Environment.UserInteractive)
    {
        configuration.WriteTo.Console(
            restrictedToMinimumLevel: LogEventLevel.Information,
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
    }

    configuration.WriteTo.File(
        logPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

    configuration.WriteTo.File(
        errorPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31,
        restrictedToMinimumLevel: LogEventLevel.Error,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

    var dashboardState = services.GetService<DashboardStateService>();
    if (dashboardState != null)
    {
        configuration.WriteTo.Sink(new DashboardLogSink(dashboardState));
    }
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
    context.Database.EnsureCreated();
    
    var settingsService = scope.ServiceProvider.GetRequiredService<AppSettingsService>();
    await settingsService.InitializeDefaultsAsync();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapApplicationEndpoints();

if (Environment.UserInteractive)
{
    Console.Clear();
    Console.WriteLine();
    Console.WriteLine(new string('=', 60));
    Console.WriteLine("  DASHBOARD WEB - Kropkontrol Store & Forward");
    Console.WriteLine(new string('=', 60));
    Console.WriteLine();
    
    // Affiche toutes les adresses disponibles
    var hostName = Dns.GetHostName();
    var ipAddresses = Dns.GetHostAddresses(hostName)
        .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
        .Select(ip => ip.ToString())
        .ToList();
    
    Console.WriteLine("  Adresses disponibles :");
    Console.WriteLine($"    → http://localhost:5017");
    
    foreach (var ip in ipAddresses.Take(5))
    {
        Console.WriteLine($"    → http://{ip}:5017");
    }
    
    Console.WriteLine();
    Console.WriteLine($"  Base de données : {paths.DbPath}");
    Console.WriteLine(new string('=', 60));
    Console.WriteLine();
    Console.WriteLine("  Appuyez sur une touche pour continuer...");
    Console.ReadKey(true);
    Console.Clear();
}

app.Run("http://0.0.0.0:5017");
