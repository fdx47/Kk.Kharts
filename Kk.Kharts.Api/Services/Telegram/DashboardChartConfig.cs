using Kk.Kharts.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kk.Kharts.Api.Services.Telegram;

/// <summary>
/// Representa uma curva/gráfico configurado no dashboard do utilizador.
/// </summary>
public class DashboardChartConfig
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("devEui")]
    public string DevEui { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("variables")]
    public List<string> Variables { get; set; } = new();

    [JsonPropertyName("intervalDays")]
    public double IntervalDays { get; set; } = 1.5;
}

/// <summary>
/// Estado do dashboard guardado pelo utilizador.
/// </summary>
public class DashboardState
{
    [JsonPropertyName("charts")]
    public List<DashboardChartConfig> Charts { get; set; } = new();
}

/// <summary>
/// Serviço para obter configurações de curvas do dashboard.
/// </summary>
public interface IDashboardConfigService
{
    Task<List<DashboardChartConfig>> GetUserChartsAsync(int userId, CancellationToken ct = default);
}

public class DashboardConfigService(
    Data.AppDbContext db,
    ILogger<DashboardConfigService> logger) : IDashboardConfigService
{
    public async Task<List<DashboardChartConfig>> GetUserChartsAsync(int userId, CancellationToken ct = default)
    {
        try
        {
            var dashboard = await db.Dashboards
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (dashboard?.StateJson == null)
                return new List<DashboardChartConfig>();

            var state = JsonSerializer.Deserialize<DashboardState>(dashboard.StateJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return state?.Charts ?? new List<DashboardChartConfig>();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Erreur lors de la lecture des charts du dashboard pour l'utilisateur {UserId}", userId);
            return new List<DashboardChartConfig>();
        }
    }
}
