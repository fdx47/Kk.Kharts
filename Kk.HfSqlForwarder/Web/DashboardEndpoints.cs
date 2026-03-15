using HfSqlForwarder.Services;
using HfSqlForwarder.Settings;
using HfSqlForwarder.State;
using HfSqlForwarder.Models;
using System.Globalization;

namespace HfSqlForwarder.Web;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => Results.Redirect("/admin"));

        app.MapGet("/admin", () => Results.Content("""
<!DOCTYPE html>
<html lang="fr" x-data="dashboard()" x-init="init()">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>HfSqlForwarder - Administration</title>
    <script defer src="https://cdn.jsdelivr.net/npm/alpinejs@3.x.x/dist/cdn.min.js"></script>
    <script src="https://cdn.tailwindcss.com"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    <style>
        [x-cloak] { display: none !important; }
        .gradient-bg { background: radial-gradient(circle at 20% 20%, rgba(59,130,246,0.15), transparent 25%), radial-gradient(circle at 80% 0%, rgba(167,139,250,0.12), transparent 25%), #0b1221; }
        .card { background: rgba(15,23,42,0.7); border: 1px solid rgba(148,163,184,0.15); box-shadow: 0 20px 60px rgba(0,0,0,0.35); }
        .glass { backdrop-filter: blur(12px); }
        .code { font-family: "Cascadia Code", "Fira Code", monospace; }
        .pill { border-radius: 9999px; }
    </style>
</head>
<body class="gradient-bg min-h-screen text-slate-100">
    <div class="max-w-7xl mx-auto px-4 py-8 space-y-8">

        <!-- Header -->
        <header class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6">
            <div class="flex items-center gap-4">
                <div class="w-14 h-14 rounded-2xl bg-gradient-to-br from-blue-500 via-cyan-400 to-purple-500 flex items-center justify-center text-2xl font-bold shadow-lg">HF</div>
                <div>
                    <h1 class="text-2xl font-semibold">HfSqlForwarder</h1>
                    <p class="text-sm text-slate-400">Transfert HFSQL vers GrowFlex · Console d'administration</p>
                    <div class="mt-1 text-xs text-slate-400 flex items-center gap-2">
                        <span class="pill px-2 py-0.5 bg-white/10 border border-white/10">Mode Free Table</span>
                        <span class="pill px-2 py-0.5 bg-white/5 border border-white/10">X-Admin-Secret</span>
                    </div>
                </div>
            </div>
            <div class="flex gap-3">
                <button @click="refreshAll" class="px-4 py-2 rounded-lg bg-white/10 hover:bg-white/15 border border-white/10 transition flex items-center gap-2">
                    <i class="fas fa-rotate"></i><span>Rafraîchir</span>
                </button>
                <button @click="logout" class="px-4 py-2 rounded-lg bg-red-500/20 hover:bg-red-500/30 border border-red-500/30 text-red-200 transition flex items-center gap-2">
                    <i class="fas fa-sign-out-alt"></i><span>Déconnexion</span>
                </button>
            </div>
        </header>

        <!-- Auth -->
        <section class="grid grid-cols-1 lg:grid-cols-3 gap-6" x-show="!authenticated">
            <div class="card glass rounded-2xl p-6 lg:col-span-2">
                <h2 class="text-lg font-semibold mb-4">Authentification</h2>
                <div class="grid md:grid-cols-2 gap-4">
                    <div>
                        <label class="text-sm text-slate-400">Secret administrateur</label>
                        <input x-model="loginForm.secret" type="password" class="mt-2 w-full rounded-lg bg-white/5 border border-white/10 px-4 py-3 focus:outline-none focus:ring-2 focus:ring-cyan-400" placeholder="X-Admin-Secret" />
                    </div>
                    <div class="flex items-end gap-3">
                        <button @click="login" class="px-4 py-3 rounded-lg bg-gradient-to-r from-cyan-500 to-blue-500 hover:from-cyan-400 hover:to-blue-400 text-white font-semibold w-full">Se connecter</button>
                    </div>
                </div>
                <p class="text-xs text-slate-400 mt-3">Le secret est envoyé dans le header <code class="code bg-white/5 px-2 py-1 rounded">X-Admin-Secret</code>.</p>
                <div x-show="loginError" class="mt-4 p-3 rounded-lg border border-red-500/40 bg-red-500/10 text-red-200 text-sm" x-text="loginError"></div>
            </div>
            <div class="card glass rounded-2xl p-6 space-y-3">
                <h3 class="text-sm text-slate-400">Raccourcis</h3>
                <button @click="prefill('25102017')" class="w-full text-left px-3 py-2 rounded-lg bg-white/5 hover:bg-white/10 border border-white/10 transition">Remplir secret (démo)</button>
                <p class="text-xs text-slate-500">Vous pouvez personnaliser cette valeur dans appsettings ou via votre système de secrets.</p>
            </div>
        </section>

        <!-- Main dashboard -->
        <div x-show="authenticated" x-cloak class="space-y-6">

            <!-- Status cards -->
            <section class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4">
                <div class="card glass rounded-2xl p-5 flex flex-col gap-2">
                    <div class="flex items-center justify-between text-sm text-slate-400">
                        <span>Connexion HFSQL</span>
                        <span :class="state.hfsqlHealthy ? 'text-green-400' : 'text-red-400'" class="pill px-2 py-0.5 bg-white/5 border border-white/10" x-text="state.hfsqlHealthy ? 'OK' : 'Erreur'"></span>
                    </div>
                    <div class="text-xl font-semibold" x-text="state.lastHealth"></div>
                    <p class="text-xs text-slate-500">Dernier check: <span x-text="formatDate(state.lastHealthTime)"></span></p>
                </div>
                <div class="card glass rounded-2xl p-5 flex flex-col gap-2">
                    <div class="flex items-center justify-between text-sm text-slate-400">
                        <span>Cycle Forwarder</span>
                        <span class="pill px-2 py-0.5 bg-white/5 border border-white/10" x-text="state.lastCycleStatus || 'N/A'"></span>
                    </div>
                    <div class="text-xl font-semibold" x-text="state.lastCycle"></div>
                    <p class="text-xs text-slate-500">Erreur: <span x-text="state.lastError || '-' "></span></p>
                </div>
                <div class="card glass rounded-2xl p-5 flex flex-col gap-2">
                    <div class="flex items-center justify-between text-sm text-slate-400">
                        <span>Config NumElt</span>
                        <span class="pill px-2 py-0.5 bg-white/5 border border-white/10" x-text="config.numelt ?? '-' "></span>
                    </div>
                    <div class="text-xl font-semibold" x-text="config.intervalMinutes ? config.intervalMinutes + ' min' : '-' "></div>
                    <p class="text-xs text-slate-500">Intervalle actuel</p>
                </div>
                <div class="card glass rounded-2xl p-5 flex flex-col gap-2">
                    <div class="flex items-center justify-between text-sm text-slate-400">
                        <span>Logs récents</span>
                        <span class="pill px-2 py-0.5 bg-white/5 border border-white/10" x-text="logs.length"></span>
                    </div>
                    <div class="text-xl font-semibold">Suivi en temps réel</div>
                    <p class="text-xs text-slate-500">Cliquer sur « Logs » pour tout voir</p>
                </div>
            </section>

            <!-- Two column: health + config -->
            <section class="grid grid-cols-1 xl:grid-cols-3 gap-6">
                <div class="card glass rounded-2xl p-6 xl:col-span-2">
                    <div class="flex items-center justify-between mb-4">
                        <div>
                            <h2 class="text-lg font-semibold">État du service</h2>
                            <p class="text-sm text-slate-400">Ping, état HFSQL, dernier cycle</p>
                        </div>
                        <div class="flex gap-2">
                            <button @click="loadHealth" class="px-3 py-2 rounded-lg bg-white/5 hover:bg-white/10 border border-white/10 text-sm">Vérifier santé</button>
                            <button @click="loadState" class="px-3 py-2 rounded-lg bg-white/5 hover:bg-white/10 border border-white/10 text-sm">État complet</button>
                        </div>
                    </div>
                    <pre class="code bg-black/50 border border-white/5 rounded-xl p-4 text-sm text-slate-200 overflow-auto max-h-72" x-text="json(stateDump)"></pre>
                </div>

                <div class="card glass rounded-2xl p-6 space-y-4">
                    <h2 class="text-lg font-semibold">Configuration</h2>
                    <label class="text-sm text-slate-400">NumElt (filtre)</label>
                    <input x-model.number="config.numelt" type="number" class="w-full rounded-lg bg-white/5 border border-white/10 px-4 py-3 focus:outline-none focus:ring-2 focus:ring-cyan-400" placeholder="0" />
                    <label class="text-sm text-slate-400">Intervalle (minutes)</label>
                    <input x-model.number="config.intervalMinutes" type="number" class="w-full rounded-lg bg-white/5 border border-white/10 px-4 py-3 focus:outline-none focus:ring-2 focus:ring-cyan-400" placeholder="3" />
                    <div class="flex gap-2 pt-2">
                        <button @click="saveConfig" class="flex-1 px-3 py-3 rounded-lg bg-gradient-to-r from-cyan-500 to-blue-500 text-white font-semibold">Sauvegarder</button>
                        <button @click="loadConfig" class="flex-1 px-3 py-3 rounded-lg bg-white/5 border border-white/10">Recharger</button>
                    </div>
                    <div x-show="configStatus" class="text-sm" :class="configStatus==='ok' ? 'text-green-300' : 'text-red-300'" x-text="configStatus==='ok' ? 'Configuration enregistrée' : 'Erreur config'"></div>
                </div>
            </section>

            <!-- Logs -->
            <section class="card glass rounded-2xl p-6">
                <div class="flex items-center justify-between mb-4">
                    <h2 class="text-lg font-semibold">Journal</h2>
                    <div class="flex gap-2">
                        <button @click="refreshLogs" class="px-3 py-2 rounded-lg bg-white/5 border border-white/10 hover:bg-white/10 text-sm">Actualiser</button>
                        <button @click="clearLogsView" class="px-3 py-2 rounded-lg bg-white/5 border border-white/10 hover:bg-white/10 text-sm">Vider affichage</button>
                    </div>
                </div>
                <div class="space-y-2 max-h-[500px] overflow-y-auto">
                    <template x-for="log in logs" :key="log.timestamp + log.message">
                        <div class="flex items-start gap-3 text-sm border-b border-white/5 pb-2">
                            <span class="text-slate-500 w-24 shrink-0" x-text="formatDateTime(log.timestamp)"></span>
                            <span class="pill px-2 py-0.5 text-xs font-semibold" :class="levelClass(log.level)" x-text="log.level"></span>
                            <span class="text-slate-200" x-text="log.message"></span>
                        </div>
                    </template>
                    <div x-show="logs.length===0" class="text-slate-500 text-sm">Aucun log</div>
                </div>
            </section>

        </div>
    </div>

    <script>
        function dashboard() {
            return {
                authenticated: false,
                loginForm: { secret: '' },
                loginError: '',
                state: { hfsqlHealthy: false, lastCycle: '-', lastCycleStatus: '', lastError: '', lastHealth: '-', lastHealthTime: null },
                stateDump: {},
                config: { numelt: null, intervalMinutes: null },
                configStatus: null,
                logs: [],
                init() {
                    const saved = localStorage.getItem('hf-admin-secret');
                    if (saved) { this.loginForm.secret = saved; this.authenticated = true; this.refreshAll(); }
                },
                prefill(val) { this.loginForm.secret = val; },
                logout() { this.authenticated = false; localStorage.removeItem('hf-admin-secret'); this.loginForm.secret = ''; },
                async login() {
                    if (!this.loginForm.secret) { this.loginError = 'Secret requis'; return; }
                    localStorage.setItem('hf-admin-secret', this.loginForm.secret);
                    this.loginError = ''; this.authenticated = true; await this.refreshAll();
                },
                headers() { return { 'X-Admin-Secret': this.loginForm.secret || localStorage.getItem('hf-admin-secret') || '', 'Content-Type': 'application/json' }; },
                async call(path, method='GET', body=null) {
                    const res = await fetch(path, { method, headers: this.headers(), body: body ? JSON.stringify(body) : null });
                    const text = await res.text();
                    let data = text; try { data = JSON.parse(text); } catch {}
                    return { ok: res.ok, data };
                },
                async loadConfig() {
                    const r = await this.call('/admin/config');
                    if (r.ok) { this.config.numelt = r.data?.NumElt ?? null; this.config.intervalMinutes = r.data?.IntervalMinutes ?? null; this.configStatus = null; }
                },
                async saveConfig() {
                    const payload = { NumElt: this.config.numelt ?? 0, IntervalMinutes: this.config.intervalMinutes ?? 3 };
                    const r = await this.call('/admin/config','POST', payload);
                    this.configStatus = r.ok ? 'ok' : 'err';
                },
                async loadHealth() {
                    const r = await this.call('/admin/health');
                    if (r.ok) {
                        this.state.hfsqlHealthy = r.data?.isHealthy ?? false;
                        this.state.lastHealth = r.data?.status ?? 'N/A';
                        this.state.lastHealthTime = r.data?.timestamp ?? null;
                    }
                    this.stateDump.health = r.data;
                },
                async loadState() {
                    const r = await this.call('/admin/state');
                    if (r.ok) {
                        this.state.lastCycle = r.data?.lastCycle ?? '-';
                        this.state.lastCycleStatus = r.data?.lastStatus ?? '';
                        this.state.lastError = r.data?.lastError ?? '';
                    }
                    this.stateDump.state = r.data;
                },
                async refreshLogs() {
                    const r = await this.call('/admin/logs');
                    if (Array.isArray(r.data)) this.logs = r.data;
                },
                clearLogsView() { this.logs = []; },
                async refreshAll() {
                    await this.loadConfig();
                    await this.loadHealth();
                    await this.loadState();
                    await this.refreshLogs();
                },
                formatDate(ts) { if (!ts) return '-'; return new Date(ts).toLocaleString(); },
                formatDateTime(ts) { if (!ts) return '-'; return new Date(ts).toLocaleString(); },
                json(obj) { return JSON.stringify(obj ?? {}, null, 2); },
                levelClass(level) {
                    const map = { INF: 'bg-green-500/20 text-green-200', WRN: 'bg-amber-500/20 text-amber-200', ERR: 'bg-red-500/20 text-red-200' };
                    return (map[level] || 'bg-white/10 text-slate-200') + ' pill px-2';
                }
            }
        }
        loadHealth();
    </script>
</body>
</html>
""", "text/html; charset=utf-8"));

        var group = app.MapGroup("/admin").AddEndpointFilter<AdminAuthFilter>();

        group.MapGet("/health", (ForwarderStateStore store) =>
        {
            var state = store.Current;
            var healthy = DateTimeOffset.UtcNow - state.LastRunUtc < TimeSpan.FromMinutes(15);
            return Results.Json(new { status = healthy ? "OK" : "WARN", lastRunUtc = state.LastRunUtc, lastError = state.LastError });
        });

        group.MapGet("/config", (RuntimeSettingsService settings) =>
        {
            return Results.Json(ToConfigResponse(settings.Get()));
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
                if (request.Filtre.NumEltList is not null && request.Filtre.NumEltList.Count > 0)
                {
                    current.Filtre.NumEltList = request.Filtre.NumEltList;
                }
            }

            // Supporte aussi les champs top-level (NumElt, IntervalMinutes) envoyés par le front simple
            if (request.NumElt.HasValue)
            {
                current.Filtre.NumElt = request.NumElt.Value;
            }
            if (request.NumEltList is not null && request.NumEltList.Count > 0)
            {
                current.Filtre.NumEltList = request.NumEltList;
            }
            if (request.IntervalMinutes.HasValue)
            {
                current.Scheduler.IntervalMinutes = request.IntervalMinutes.Value;
            }

            if (request.State is not null)
            {
                current.State.StatePath = request.State.StatePath ?? current.State.StatePath;
            }

            settings.Update(current);
            return Results.Json(new { message = "Config mise à jour", config = ToConfigResponse(current) });
        });

        group.MapGet("/logs", () => Results.Json(Array.Empty<object>()));

        group.MapGet("/rega", (string? jour, HfSqlReader reader) =>
        {
            DateOnly date;
            if (!string.IsNullOrWhiteSpace(jour) && DateOnly.TryParseExact(jour, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                date = parsed;
            }
            else
            {
                date = DateOnly.FromDateTime(DateTime.Now);
            }

            var items = reader.ReadSync(date).ToList();
            return Results.Json(new { day = date.ToString("yyyy-MM-dd"), count = items.Count, items });
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
            new FiltreConfig(opts.Filtre.NumElt, opts.Filtre.NumEltList ?? new List<int>()),
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

public record FiltreConfig(int NumElt, List<int> NumEltList);

public record StateConfig(string StatePath);

public class ConfigUpdateRequest
{
    public HfSqlConfigUpdate? HfSql { get; set; }
    public SchedulerConfigUpdate? Scheduler { get; set; }
    public ApiConfigUpdate? Api { get; set; }
    public FiltreConfigUpdate? Filtre { get; set; }
    public StateConfigUpdate? State { get; set; }

    // Champs top-level utilisés par la page HTML simple
    public int? NumElt { get; set; }
    public List<int>? NumEltList { get; set; }
    public int? IntervalMinutes { get; set; }
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
    public List<int>? NumEltList { get; set; }
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
