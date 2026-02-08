using KK.UG6x.StoreAndForward.Contracts;
using KK.UG6x.StoreAndForward.Domain.Interfaces;
using KK.UG6x.StoreAndForward.Services;
using System.Text.Json;

namespace KK.UG6x.StoreAndForward.Endpoints;

public static class ApiEndpoints
{
    public static void MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (HttpContext ctx) =>
        {
            var body = await ctx.Request.ReadFromJsonAsync<LoginRequest>();
            if (body?.Username == "admin" && body?.Password == "kropkontrol")
            {
                var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{body.Username}:{DateTime.UtcNow.Ticks}"));
                return Results.Ok(new { token });
            }
            return Results.Unauthorized();
        });

        app.MapGet("/api/dashboard", (DashboardStateService state) =>
        {
            var data = state.GetDashboardData();
            return Results.Ok(new
            {
                isOnline = data.IsOnline,
                internetAvailable = data.InternetAvailable,
                isForcedOffline = data.IsForcedOffline,
                isOfflineOnlyMode = data.IsOfflineOnlyMode,
                gatewayUrl = data.GatewayUrl,
                lastSync = data.LastSync,
                lastBatchCount = data.LastBatchCount,
                pendingCount = data.PendingCount,
                mappedDevicesCount = data.MappedDevicesCount,
                serviceStartTime = data.ServiceStartTime,
                uptime = data.Uptime.ToString(@"hh\:mm\:ss")
            });
        }).RequireAuthorization();

        app.MapGet("/api/logs", (DashboardStateService state) =>
        {
            return Results.Ok(state.GetLogs(200));
        }).RequireAuthorization();

        app.MapGet("/api/settings", async (AppSettingsService settingsService) =>
        {
            var gateway = await settingsService.GetSettingsByCategoryAsync("GatewaySettings");
            var worker = await settingsService.GetSettingsByCategoryAsync("WorkerSettings");

            var forceOfflineValue = worker.GetValueOrDefault("ForceOffline", "false");
            var forceOffline = bool.TryParse(forceOfflineValue, out var forceOfflineParsed) && forceOfflineParsed;

            var offlineOnlyModeValue = worker.GetValueOrDefault("OfflineOnlyMode", "false");
            var offlineOnlyMode = bool.TryParse(offlineOnlyModeValue, out var offlineOnlyModeParsed) && offlineOnlyModeParsed;

            return Results.Ok(new
            {
                gatewayUsername = gateway.GetValueOrDefault("Username", ""),
                gatewayPassword = gateway.GetValueOrDefault("Password", ""),
                intervalSeconds = int.TryParse(worker.GetValueOrDefault("IntervalSeconds", "60"), out var i) ? i : 60,
                internetCheckHost = worker.GetValueOrDefault("InternetCheckHost", "8.8.8.8"),
                forceOffline,
                offlineOnlyMode
            });
        }).RequireAuthorization();



        app.MapPost("/api/settings", async (HttpContext ctx, AppSettingsService settingsService, DashboardStateService dashboardState) =>
        {
            var body = await JsonSerializer.DeserializeAsync<SettingsRequest>(ctx.Request.Body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (body == null) return Results.BadRequest();

            await settingsService.SaveSettingAsync("Username", body.GatewayUsername ?? "", "GatewaySettings", false);
            await settingsService.SaveSettingAsync("Password", body.GatewayPassword ?? "", "GatewaySettings", true);
            var normalizedInterval = body.IntervalSeconds > 0 ? body.IntervalSeconds : 60;
            await settingsService.SaveSettingAsync("IntervalSeconds", normalizedInterval.ToString(), "WorkerSettings", false);
            await settingsService.SaveSettingAsync("InternetCheckHost", body.InternetCheckHost ?? "8.8.8.8", "WorkerSettings", false);
            await settingsService.SaveSettingAsync("ForceOffline", body.ForceOffline.ToString(), "WorkerSettings", false);
            await settingsService.SaveSettingAsync("OfflineOnlyMode", body.OfflineOnlyMode.ToString(), "WorkerSettings", false);

            dashboardState.AddLog("INF", "Configuration mise à jour via l'interface web");
            return Results.Ok();
        }).RequireAuthorization();

        app.MapPost("/api/sync/purge", async (ILocalStore localStore, DashboardStateService dashboardState) =>
        {
            await localStore.ClearAllAsync();
            dashboardState.AddLog("INF", "Purge manuelle des données locales effectuée.");
            return Results.Ok(new { message = "Local data cleared" });
        }).RequireAuthorization();

        app.MapPost("/api/gateway/purge", async (IUG65Client ug65Client, DashboardStateService dashboardState, CancellationToken ct) =>
        {
            var success = await ug65Client.PurgeUrPacketsAsync(ct);
            if (success)
            {
                dashboardState.AddLog("INF", "Purge manuelle déclenchée sur la passerelle UG65 (UR Packets).");
                return Results.Ok(new { message = "Gateway buffer cleared" });
            }

            dashboardState.AddLog("WRN", "Échec lors de la purge des données UG65.");
            return Results.StatusCode(StatusCodes.Status502BadGateway);
        }).RequireAuthorization();
    }
}
