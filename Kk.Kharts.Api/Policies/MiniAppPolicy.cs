using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Kk.Kharts.Api.Policies;

/// <summary>
/// Política CORS para a Telegram Mini App hospedada externamente.
/// </summary>
public static class MiniAppPolicy
{
    public static void AddMiniAppPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("MiniApp", policy =>
            {
                policy.WithOrigins(
                        "https://kropkontrol.com",
                        "https://www.kropkontrol.com"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });

            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }
}
