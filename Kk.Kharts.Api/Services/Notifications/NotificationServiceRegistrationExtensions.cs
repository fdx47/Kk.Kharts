using Kk.Kharts.Api.Services.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class NotificationServiceRegistrationExtensions
{
    public static IServiceCollection AddNotificationSystem(this IServiceCollection services)
    {
        services.AddSingleton<ITelegramNotificationRenderer, TelegramNotificationRenderer>();
        services.AddSingleton<IPushoverNotificationRenderer, PushoverNotificationRenderer>();
        services.AddSingleton<AlertNotificationFactory>();

        services.AddScoped<INotificationChannel, TelegramNotificationChannel>();
        services.AddScoped<INotificationChannel, PushoverNotificationChannel>();
        services.AddScoped<INotificationChannel, EmailNotificationChannel>();

        services.AddScoped<INotificationRouter, NotificationRouter>();

        return services;
    }
}
