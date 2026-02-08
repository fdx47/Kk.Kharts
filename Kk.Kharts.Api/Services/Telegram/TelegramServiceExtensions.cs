using Kk.Kharts.Api.Services.Telegram.Commands;
using Kk.Kharts.Api.Services.Telegram.Commands.Callbacks;
using Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

namespace Kk.Kharts.Api.Services.Telegram;

/// <summary>
/// Extensões para registar o serviço de Telegram no DI container.
/// </summary>
public static class TelegramServiceExtensions
{
    /// <summary>
    /// Adiciona o serviço de Telegram ao container de DI.
    /// </summary>
    public static IServiceCollection AddTelegramServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuração
        services.Configure<TelegramOptions>(
            configuration.GetSection(TelegramOptions.SectionName));

        // Serviço principal
        services.AddSingleton<ITelegramService, TelegramService>();

        // Serviço de utilizadores Telegram
        services.AddScoped<ITelegramUserService, TelegramUserService>();

        // Serviço de gráficos
        services.AddScoped<ITelegramChartService, TelegramChartService>();

        // Serviço de configuração de dashboard
        services.AddScoped<IDashboardConfigService, DashboardConfigService>();


        // Serviço de notificações de alarmes
        services.AddScoped<ITelegramAlarmNotificationService, TelegramAlarmNotificationService>();

        // Command Handlers
        services.AddScoped<ITelegramCommandHandler, StartCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, HelpCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, AideCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, DevicesCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, StatusCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, UsersStatsCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, LastCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, BatteryCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, OfflineCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, AlertsCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, ChartCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, LinkCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, UnlinkCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, SupportCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, AppCommandHandler>();

        // Aliases en français
        services.AddScoped<ITelegramCommandHandler, StatutCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, CapteursCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, DernierCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, HorsligneCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, BatterieCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, GraphiqueCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, AlertesCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, LierCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, DelierCommandHandler>();

        // Registar handlers individuais para injeção direta
        services.AddScoped<StartCommandHandler>();
        services.AddScoped<HelpCommandHandler>();
        services.AddScoped<AideCommandHandler>();
        services.AddScoped<DevicesCommandHandler>();
        services.AddScoped<StatusCommandHandler>();
        services.AddScoped<UsersStatsCommandHandler>();
        services.AddScoped<LastCommandHandler>();
        services.AddScoped<BatteryCommandHandler>();
        services.AddScoped<OfflineCommandHandler>();
        services.AddScoped<AlertsCommandHandler>();
        services.AddScoped<ChartCommandHandler>();
        services.AddScoped<LinkCommandHandler>();
        services.AddScoped<UnlinkCommandHandler>();
        services.AddScoped<SupportCommandHandler>();
        services.AddScoped<AppCommandHandler>();

        // Callback Handlers
        services.AddScoped<ITelegramCallbackHandler, MenuCallbackHandler>();
        services.AddScoped<ITelegramCallbackHandler, DeviceCallbackHandler>();
        services.AddScoped<ITelegramCallbackHandler, BackCallbackHandler>();
        services.AddScoped<ITelegramCallbackHandler, ChartCallbackHandler>();
        services.AddScoped<ITelegramCallbackHandler, PageCallbackHandler>();
        services.AddScoped<ITelegramCallbackHandler, DeviceTypeCallbackHandler>();
        services.AddScoped<ITelegramCallbackHandler, ChartTypeCallbackHandler>();
        services.AddScoped<ITelegramCallbackHandler, RefreshCallbackHandler>();
        services.AddScoped<ITelegramCallbackHandler, AlertsCallbackHandler>();
        services.AddScoped<ITelegramCallbackHandler, FilterCallbackHandler>();
        services.AddScoped<ITelegramCallbackHandler, LastCallbackHandler>();
        services.AddScoped<ITelegramCallbackHandler, DashboardChartsCallbackHandler>();

        // Command Dispatcher
        services.AddScoped<TelegramCommandDispatcher>();

        return services;
    }
}
