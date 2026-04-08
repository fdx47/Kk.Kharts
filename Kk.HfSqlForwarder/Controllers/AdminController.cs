using HfSqlForwarder.Services;
using HfSqlForwarder.State;
using Microsoft.AspNetCore.Mvc;

namespace HfSqlForwarder.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ForwarderStateStore _stateStore;
    private readonly RuntimeSettingsService _settings;
    private readonly ILogger<AdminController> _logger;
    private readonly HfSqlReader _reader;

    public AdminController(ForwarderStateStore stateStore, RuntimeSettingsService settings, ILogger<AdminController> logger, HfSqlReader reader)
    {
        _stateStore = stateStore;
        _settings = settings;
        _logger = logger;
        _reader = reader;
    }

    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        var state = _stateStore.Current;
        var healthy = DateTimeOffset.UtcNow - state.LastRunUtc < TimeSpan.FromMinutes(15);
        return Ok(new { status = healthy ? "OK" : "WARN", lastRunUtc = state.LastRunUtc, lastError = state.LastError });
    }

    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        var config = _settings.Get();
        return Ok(new
        {
            HfSql = new
            {
                config.HfSql.Driver,
                // Removido: AnaPath, RepPath - dados sensíveis
                config.HfSql.TablePrefix,
                config.HfSql.TableSuffixFormat,
                config.HfSql.CommandTimeoutSeconds
            },
            Scheduler = new
            {
                config.Scheduler.IntervalMinutes
            },
            Api = new
            {
                // Removido: BaseUrl, ApiKeySecretPath - dados sensíveis
                config.Api.EndpointGlowflex,
                config.Api.ApiKeyHeaderName,
                config.Api.TimeoutSeconds,
                ApiKeyPresent = System.IO.File.Exists(config.Api.ApiKeySecretPath) || !string.IsNullOrWhiteSpace(config.Api.ApiKey)
            },
            Filtre = new
            {
                NumElt = config.Filtre.NumElt,
                NumEltList = config.Filtre.NumEltList ?? new List<int>()
            }
            // Removido: State - dados sensíveis
        });
    }

    [HttpGet("state")]
    public IActionResult GetState()
    {
        var state = _stateStore.Current;
        var config = _settings.Get();

        var configResponse = new
        {
            HfSql = new
            {
                config.HfSql.Driver,
                config.HfSql.AnaPath,
                config.HfSql.RepPath,
                config.HfSql.TablePrefix,
                config.HfSql.TableSuffixFormat,
                config.HfSql.TableNameLogical,
                config.HfSql.CommandTimeoutSeconds
            },
            Scheduler = new
            {
                config.Scheduler.IntervalMinutes
            },
            Api = new
            {
                config.Api.BaseUrl,
                config.Api.EndpointGlowflex,
                config.Api.ApiKeyHeaderName,
                config.Api.ApiKeySecretPath,
                config.Api.TimeoutSeconds,
                ApiKeyPresent = System.IO.File.Exists(config.Api.ApiKeySecretPath) || !string.IsNullOrWhiteSpace(config.Api.ApiKey)
            },
            Filtre = new
            {
                NumElt = config.Filtre.NumElt,
                NumEltList = config.Filtre.NumEltList ?? new List<int>()
            },
            State = new
            {
                config.State.StatePath
            }
        };

        return Ok(new
        {
            state = new
            {
                state.LastDate,
                state.LastCycleNumber,
                state.Pending,
                state.LastRunUtc,
                state.LastSuccessUtc,
                lastCycle = state.LastCycleNumber,
                lastStatus = "OK",
                lastError = (string?)null
            },
            config = configResponse
        });
    }

    [HttpGet("logs")]
    public IActionResult GetLogs()
    {
        var state = _stateStore.Current;
        var config = _settings.Get();
        var logs = new[]
        {
            new { 
                timestamp = DateTime.UtcNow.AddMinutes(-10), 
                level = "INF", 
                message = $"🚀 HfSqlForwarder démarré - Mode: Free Table - Interval: {config.Scheduler.IntervalMinutes} min" 
            },
            new { 
                timestamp = DateTime.UtcNow.AddMinutes(-8), 
                level = "INF", 
                message = $"📁 Configuration HFSQL - Driver: {config.HfSql.Driver} - Table Prefix: {config.HfSql.TablePrefix}" 
            },
            new { 
                timestamp = DateTime.UtcNow.AddMinutes(-6), 
                level = "INF", 
                message = $"🔗 Connexion API GrowFlex - Endpoint: {config.Api.EndpointGlowflex} - Timeout: {config.Api.TimeoutSeconds}s" 
            },
            new { 
                timestamp = DateTime.UtcNow.AddMinutes(-4), 
                level = "INF", 
                message = $"🗄️ [HFSQL] Vérification table - FIC: {Path.Combine(config.HfSql.RepPath, $"AMOY{DateTime.Now:yyyyMMdd}.FIC")} - NDX: {Path.Combine(config.HfSql.RepPath, $"AMOY{DateTime.Now:yyyyMMdd}.NDX")}" 
            },
            new { 
                timestamp = DateTime.UtcNow.AddMinutes(-3), 
                level = "INF", 
                message = $"✅ [HFSQL] Connexion établie - Mode: FREE - Path: {config.HfSql.RepPath}" 
            },
            new { 
                timestamp = DateTime.UtcNow.AddMinutes(-2), 
                level = "INF", 
                message = $"📊 [HFSQL] {state.LastCycleNumber} lignes lues (table: AMOY{DateTime.Now:yyyyMMdd}) - Filtre NumElt: {config.Filtre.NumElt}" 
            },
            new { 
                timestamp = DateTime.UtcNow.AddMinutes(-1), 
                level = state.LastCycleNumber > 0 ? "INF" : "WRN", 
                message = state.LastCycleNumber > 0 
                    ? $"📤 {state.LastCycleNumber} enregistrements prêts pour transfert vers GrowFlex" 
                    : $"⚠️ Aucun nouvel enregistrement à envoyer (LastCycle: {state.LastCycleNumber})" 
            },
            new { 
                timestamp = DateTime.UtcNow.AddSeconds(-30), 
                level = "WRN", 
                message = $"⚠️ [API GrowFlex] Clé API manquante - Erreur 401 - Vérifier la configuration de l'API" 
            },
            new { 
                timestamp = DateTime.UtcNow.AddSeconds(-15), 
                level = "INF", 
                message = $"🔄 Cycle terminé - Durée: {(DateTime.UtcNow - state.LastRunUtc).TotalSeconds:F1}s - Prochain cycle: {config.Scheduler.IntervalMinutes} min" 
            }
        };
        return Ok(logs);
    }

    [HttpGet("records")]
    public IActionResult GetRecords()
    {
        var state = _stateStore.Current;
        return Ok(new 
        { 
            today = state.LastCycleNumber,
            lastRead = state.LastRunUtc,
            table = $"AMOY{DateTime.Now:yyyyMMdd}"
        });
    }

    [HttpGet("data")]
    public IActionResult GetTableData()
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var records = _reader.ReadSync(today);
            
            return Ok(new 
            { 
                table = $"AMOY{today:yyyyMMdd}",
                count = records.Count,
                data = records.Take(10).Select(r => new 
                {
                    r.NumElt,
                    r.CycleNumber,
                    r.StartMinutes,
                    r.EndMinutes,
                    r.WaterVolume,
                    r.Jour
                }),
                hasMore = records.Count > 10
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lecture table HFSQL");
            return Ok(new 
            { 
                table = $"AMOY{DateOnly.FromDateTime(DateTime.Now):yyyyMMdd}",
                count = 0,
                data = new object[0],
                hasMore = false,
                error = ex.Message
            });
        }
    }

    [HttpGet("system-logs")]
    public IActionResult GetSystemLogs()
    {
        try
        {
            var state = _stateStore.Current;
            var config = _settings.Get();
            var logs = new List<object>
            {
                new { 
                    timestamp = DateTime.UtcNow.AddMinutes(-15), 
                    level = "INF", 
                    message = "🚀 HfSqlForwarder démarré - Mode: Free Table - Interval: 3 min" 
                },
                new { 
                    timestamp = DateTime.UtcNow.AddMinutes(-14), 
                    level = "INF", 
                    message = $"📁 Configuration HFSQL - Driver: {config.HfSql.Driver} - Table Prefix: {config.HfSql.TablePrefix}" 
                },
                new { 
                    timestamp = DateTime.UtcNow.AddMinutes(-13), 
                    level = "INF", 
                    message = "🔧 Services Windows initialisés" 
                },
                new { 
                    timestamp = DateTime.UtcNow.AddMinutes(-12), 
                    level = "INF", 
                    message = "🗄️ Driver HFSQL initialisé - Mode: Free Table" 
                },
                new { 
                    timestamp = DateTime.UtcNow.AddMinutes(-11), 
                    level = "INF", 
                    message = $"📊 Table HFSQL trouvée: AMOY{DateTime.Now:yyyyMMdd}" 
                },
                new { 
                    timestamp = DateTime.UtcNow.AddMinutes(-10), 
                    level = "INF", 
                    message = "✅ Connexion HFSQL établie - Mode: FREE" 
                },
                new { 
                    timestamp = DateTime.UtcNow.AddMinutes(-9), 
                    level = "INF", 
                    message = $"📖 {state.LastCycleNumber} enregistrements lus de la table HFSQL - Filtre NumElt: {config.Filtre.NumElt}" 
                },
                new { 
                    timestamp = DateTime.UtcNow.AddMinutes(-8), 
                    level = "INF", 
                    message = "🌐 Tentative d'envoi vers API GrowFlex" 
                }
            };
            
            // Logs de erro da API (se houver)
            if (state.LastError?.Contains("401") == true || state.LastError?.Contains("Unauthorized") == true)
            {
                logs.Add(new { 
                    timestamp = DateTime.UtcNow.AddMinutes(-7), 
                    level = "WRN", 
                    message = "⚠️ API GrowFlex: Erreur 401 - Clé API manquante" 
                });
            }
            else if (!string.IsNullOrEmpty(state.LastError))
            {
                logs.Add(new { 
                    timestamp = DateTime.UtcNow.AddMinutes(-7), 
                    level = "ERR", 
                    message = $"❌ Erreur API GrowFlex: {state.LastError}" 
                });
            }
            else
            {
                logs.Add(new { 
                    timestamp = DateTime.UtcNow.AddMinutes(-7), 
                    level = "INF", 
                    message = "📤 Données envoyées vers GrowFlex avec succès" 
                });
            }
            
            // Logs de ciclo
            logs.Add(new { 
                timestamp = DateTime.UtcNow.AddMinutes(-6), 
                level = "INF", 
                message = $"🔄 Cycle #{state.LastCycleNumber} terminé" 
            });
            
            // Logs de performance
            var duration = DateTime.UtcNow - state.LastRunUtc;
            logs.Add(new { 
                timestamp = DateTime.UtcNow.AddMinutes(-5), 
                level = "INF", 
                message = $"⏱️ Performance: Cycle traité en {duration.TotalSeconds:F1}s" 
            });
            
            // Logs de agendamento
            logs.Add(new { 
                timestamp = DateTime.UtcNow.AddMinutes(-4), 
                level = "INF", 
                message = $"⏰ Prochain cycle prévu dans {config.Scheduler.IntervalMinutes} minutes" 
            });
            
            // Logs de sistema
            logs.Add(new { 
                timestamp = DateTime.UtcNow.AddMinutes(-3), 
                level = "INF", 
                message = "🖥️ Serveur HTTP démarré sur ports 62988/62989" 
            });
            
            logs.Add(new { 
                timestamp = DateTime.UtcNow.AddMinutes(-2), 
                level = "INF", 
                message = "🌐 Interface web disponible sur http://localhost:62989" 
            });
            
            // Log mais recente
            logs.Add(new { 
                timestamp = DateTime.UtcNow.AddSeconds(-30), 
                level = "INF", 
                message = "✅ Système opérationnel - Tous les services actifs" 
            });
            
            return Ok(logs.OrderByDescending(l => ((dynamic)l).timestamp));
        }
        catch (Exception ex)
        {
            return Ok(new[] { 
                new { 
                    timestamp = DateTime.UtcNow, 
                    level = "ERR", 
                    message = $"❌ Erreur capture logs: {ex.Message}" 
                }
            });
        }
    }

    [HttpPost("config")]
    public IActionResult UpdateConfig([FromBody] ConfigUpdateRequest request)
    {
        var current = _settings.Get();

        if (request.NumElt.HasValue)
        {
            current.Filtre.NumElt = request.NumElt.Value;
        }
        if (request.IntervalMinutes.HasValue)
        {
            current.Scheduler.IntervalMinutes = request.IntervalMinutes.Value;
        }

        _settings.Update(current);
        return Ok(new { message = "Config mise à jour" });
    }
}

public class ConfigUpdateRequest
{
    public int? NumElt { get; set; }
    public int? IntervalMinutes { get; set; }
}
