using HfSqlForwarder.Settings;
using HfSqlForwarder.State;
using HfSqlForwarder.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace HfSqlForwarder.Web;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/admin").AddEndpointFilter<AdminAuthFilter>();

        group.MapGet("/health", (ForwarderStateStore store) =>
        {
            var state = store.Current;
            var healthy = DateTimeOffset.UtcNow - state.LastRunUtc < TimeSpan.FromMinutes(15);
            return Results.Json(new { status = healthy ? "OK" : "WARN", lastRunUtc = state.LastRunUtc, lastError = state.LastError });
        });

        group.MapGet("/state", (ForwarderStateStore store, RuntimeSettingsService settings) =>
        {
            return Results.Json(new
            {
                state = store.Current,
                config = ToConfigResponse(settings.Get())
            });
        });

        group.MapPost("/config", (ConfigUpdateRequest request, RuntimeSettingsService settings) =>
        {
            var current = settings.Get();

            if (request.HfSql is not null)
            {
                current.HfSql.Driver = request.HfSql.Driver ?? current.HfSql.Driver;
                current.HfSql.AnaPath = request.HfSql.AnaPath ?? current.HfSql.AnaPath;
                current.HfSql.RepPath = request.HfSql.RepPath ?? current.HfSql.RepPath;
                current.HfSql.TablePrefix = request.HfSql.TablePrefix ?? current.HfSql.TablePrefix;
                current.HfSql.TableSuffixFormat = request.HfSql.TableSuffixFormat ?? current.HfSql.TableSuffixFormat;
                current.HfSql.TableNameLogical = request.HfSql.TableNameLogical ?? current.HfSql.TableNameLogical;
                current.HfSql.CommandTimeoutSeconds = request.HfSql.CommandTimeoutSeconds ?? current.HfSql.CommandTimeoutSeconds;
            }

            if (request.Scheduler is not null)
            {
                current.Scheduler.IntervalMinutes = request.Scheduler.IntervalMinutes ?? current.Scheduler.IntervalMinutes;
            }

            if (request.Api is not null)
            {
                current.Api.BaseUrl = request.Api.BaseUrl ?? current.Api.BaseUrl;
                current.Api.EndpointGlowflex = request.Api.EndpointGlowflex ?? current.Api.EndpointGlowflex;
                current.Api.ApiKeyHeaderName = request.Api.ApiKeyHeaderName ?? current.Api.ApiKeyHeaderName;
                current.Api.ApiKeySecretPath = request.Api.ApiKeySecretPath ?? current.Api.ApiKeySecretPath;
                current.Api.TimeoutSeconds = request.Api.TimeoutSeconds ?? current.Api.TimeoutSeconds;
                if (!string.IsNullOrWhiteSpace(request.Api.ApiKey))
                {
                    WriteApiKeySecret(current.Api.ApiKeySecretPath, request.Api.ApiKey);
                }
            }

            if (request.Filtre is not null)
            {
                current.Filtre.NumElt = request.Filtre.NumElt ?? current.Filtre.NumElt;
            }

            if (request.State is not null)
            {
                current.State.StatePath = request.State.StatePath ?? current.State.StatePath;
            }

            settings.Update(current);
            return Results.Json(new { message = "Config mise à jour", config = ToConfigResponse(current) });
        });
    }

    private static void WriteApiKeySecret(string path, string value)
    {
        var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllText(fullPath, value);
    }

    private static ConfigResponse ToConfigResponse(ForwarderOptions opts)
        => new(
            new HfSqlConfig(opts.HfSql.Driver, opts.HfSql.AnaPath, opts.HfSql.RepPath, opts.HfSql.TablePrefix, opts.HfSql.TableSuffixFormat, opts.HfSql.TableNameLogical, opts.HfSql.CommandTimeoutSeconds),
            new SchedulerConfig(opts.Scheduler.IntervalMinutes),
            new ApiConfig(opts.Api.BaseUrl, opts.Api.EndpointGlowflex, opts.Api.ApiKeyHeaderName, opts.Api.ApiKeySecretPath, opts.Api.TimeoutSeconds, File.Exists(opts.Api.ApiKeySecretPath) || !string.IsNullOrWhiteSpace(opts.Api.ApiKey)),
            new FiltreConfig(opts.Filtre.NumElt),
            new StateConfig(opts.State.StatePath)
        );
}

public record ConfigResponse(
    HfSqlConfig HfSql,
    SchedulerConfig Scheduler,
    ApiConfig Api,
    FiltreConfig Filtre,
    StateConfig State
);

public record HfSqlConfig(
    string Driver,
    string AnaPath,
    string RepPath,
    string TablePrefix,
    string TableSuffixFormat,
    string TableNameLogical,
    int CommandTimeoutSeconds);

public record SchedulerConfig(int IntervalMinutes);

public record ApiConfig(
    string BaseUrl,
    string EndpointGlowflex,
    string ApiKeyHeaderName,
    string ApiKeySecretPath,
    int TimeoutSeconds,
    bool ApiKeyPresent);

public record FiltreConfig(int NumElt);

public record StateConfig(string StatePath);

public class ConfigUpdateRequest
{
    public HfSqlConfigUpdate? HfSql { get; set; }
    public SchedulerConfigUpdate? Scheduler { get; set; }
    public ApiConfigUpdate? Api { get; set; }
    public FiltreConfigUpdate? Filtre { get; set; }
    public StateConfigUpdate? State { get; set; }
}

public class HfSqlConfigUpdate
{
    public string? Driver { get; set; }
    public string? AnaPath { get; set; }
    public string? RepPath { get; set; }
    public string? TablePrefix { get; set; }
    public string? TableSuffixFormat { get; set; }
    public string? TableNameLogical { get; set; }
    public int? CommandTimeoutSeconds { get; set; }
}

public class SchedulerConfigUpdate
{
    public int? IntervalMinutes { get; set; }
}

public class ApiConfigUpdate
{
    public string? BaseUrl { get; set; }
    public string? EndpointGlowflex { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiKeyHeaderName { get; set; }
    public string? ApiKeySecretPath { get; set; }
    public int? TimeoutSeconds { get; set; }
}

public class FiltreConfigUpdate
{
    public int? NumElt { get; set; }
}

public class StateConfigUpdate
{
    public string? StatePath { get; set; }
}

public class AdminAuthFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var settings = context.HttpContext.RequestServices.GetRequiredService<RuntimeSettingsService>();
        var opts = settings.Get();
        var headerName = opts.Admin.HeaderName ?? "X-Admin-Secret";
        if (!context.HttpContext.Request.Headers.TryGetValue(headerName, out var provided) || provided.Count == 0)
        {
            return Results.Unauthorized();
        }

        if (!string.Equals(provided.FirstOrDefault(), opts.Admin.Secret, StringComparison.Ordinal))
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}
