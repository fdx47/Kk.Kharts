using Kk.Kharts.Api.Data;
using Kk.Kharts.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kk.Kharts.Api.Services.Telegram;

/// <summary>
/// Service pour générer des graphiques via QuickChart.io API.
/// </summary>
public interface ITelegramChartService
{
    Task<Stream?> GenerateChartAsync(
        string devEui,
        string chartType,
        string period,
        CancellationToken ct = default);
}

public class TelegramChartService(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    ILogger<TelegramChartService> logger) : ITelegramChartService
{
    private const string QuickChartUrl = "https://quickchart.io/chart";

    public async Task<Stream?> GenerateChartAsync(
        string devEui,
        string chartType,
        string period,
        CancellationToken ct = default)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var device = await db.Devices.FirstOrDefaultAsync(d => d.DevEui == devEui, ct);
            if (device == null) return null;

            var (startDate, endDate) = GetDateRange(period);

            var chartData = await GetChartDataAsync(db, devEui, chartType, startDate, endDate, ct);
            if (chartData.Labels.Count == 0) return null;

            var chartConfig = BuildChartConfig(device.Name, chartType, chartData);
            
            return await FetchChartImageAsync(chartConfig, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la génération du graphique pour {DevEui}", devEui);
            return null;
        }
    }

    private static (DateTime Start, DateTime End) GetDateRange(string period)
    {
        var end = DateTime.UtcNow;
        var start = period switch
        {
            TelegramConstants.Periods.Last6Hours => end.AddHours(-6),
            TelegramConstants.Periods.Last12Hours => end.AddHours(-12),
            TelegramConstants.Periods.Last24Hours => end.AddHours(-24),
            TelegramConstants.Periods.Last36Hours => end.AddHours(-36),
            TelegramConstants.Periods.Last48Hours => end.AddHours(-48),
            TelegramConstants.Periods.Last7Days => end.AddDays(-7),
            TelegramConstants.Periods.Last30Days => end.AddDays(-30),
            _ => end.AddHours(-36)
        };
        return (start, end);
    }

    private static async Task<ChartDataResult> GetChartDataAsync(
        AppDbContext db,
        string devEui,
        string chartType,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct)
    {
        var result = new ChartDataResult();

        // Récupérer les données WET150 (UC502)
        var wet150Data = await db.Uc502Wet150s
            .Where(r => r.DevEui == devEui && r.Timestamp >= startDate && r.Timestamp <= endDate)
            .OrderBy(r => r.Timestamp)
            .ToListAsync(ct);

        if (wet150Data.Count > 0)
        {
            // Échantillonner si trop de points (max 100 pour lisibilité)
            var sampledData = SampleData(wet150Data, 100);

            foreach (var reading in sampledData)
            {
                result.Labels.Add(reading.Timestamp.ToString("dd/MM HH:mm"));

                switch (chartType)
                {
                    case TelegramConstants.ChartTypes.Temperature:
                        result.Values.Add((double)reading.SoilTemperature);
                        result.DatasetLabel = "Température Sol (°C)";
                        result.BorderColor = "#E74C3C"; // Vermelho - Temperatura
                        break;
                    case TelegramConstants.ChartTypes.VWC:
                        result.Values.Add((double)reading.MineralVWC);
                        result.DatasetLabel = "VWC Minéral (%)";
                        result.BorderColor = "#3498DB"; // Azul - Humidade/VWC
                        break;
                    case TelegramConstants.ChartTypes.EC:
                        result.Values.Add((double)reading.MineralECp);
                        result.DatasetLabel = "EC Minéral (mS/cm)";
                        result.BorderColor = "#F39C12"; // Laranja - EC
                        break;
                    default:
                        result.Values.Add((double)reading.SoilTemperature);
                        result.DatasetLabel = "Température Sol (°C)";
                        result.BorderColor = "#E74C3C"; // Vermelho - Temperatura
                        break;
                }
            }
            return result;
        }

        // Essayer EM300-TH si pas de données WET150
        var em300Data = await db.Em300ths
            .Where(r => r.DevEui == devEui && r.Timestamp >= startDate && r.Timestamp <= endDate)
            .OrderBy(r => r.Timestamp)
            .ToListAsync(ct);

        if (em300Data.Count > 0)
        {
            var sampledData = SampleData(em300Data, 100);

            foreach (var reading in sampledData)
            {
                result.Labels.Add(reading.Timestamp.ToString("dd/MM HH:mm"));

                switch (chartType)
                {
                    case TelegramConstants.ChartTypes.Temperature:
                        result.Values.Add((double)reading.Temperature);
                        result.DatasetLabel = "Température (°C)";
                        result.BorderColor = "#E74C3C"; // Vermelho - Temperatura
                        break;
                    case TelegramConstants.ChartTypes.Humidity:
                        result.Values.Add((double)reading.Humidity);
                        result.DatasetLabel = "Humidité (%)";
                        result.BorderColor = "#27AE60"; // Verde - Humidade
                        break;
                    default:
                        result.Values.Add((double)reading.Temperature);
                        result.DatasetLabel = "Température (°C)";
                        result.BorderColor = "#E74C3C"; // Vermelho - Temperatura
                        break;
                }
            }
        }

        return result;
    }

    private static List<T> SampleData<T>(List<T> data, int maxPoints)
    {
        if (data.Count <= maxPoints) return data;

        var step = (double)data.Count / maxPoints;
        var result = new List<T>();

        for (var i = 0; i < maxPoints; i++)
        {
            var index = (int)(i * step);
            if (index < data.Count)
                result.Add(data[index]);
        }

        return result;
    }

    private static object BuildChartConfig(string deviceName, string chartType, ChartDataResult data)
    {
        return new
        {
            type = "line",
            data = new
            {
                labels = data.Labels,
                datasets = new[]
                {
                    new
                    {
                        label = data.DatasetLabel,
                        data = data.Values,
                        borderColor = data.BorderColor,
                        backgroundColor = data.BorderColor + "33",
                        fill = true,
                        tension = 0.3,
                        pointRadius = 2
                    }
                }
            },
            options = new
            {
                responsive = true,
                plugins = new
                {
                    title = new
                    {
                        display = true,
                        text = $"{deviceName} - {data.DatasetLabel}",
                        font = new { size = 16 }
                    },
                    legend = new
                    {
                        display = true,
                        position = "bottom"
                    }
                },
                scales = new
                {
                    x = new
                    {
                        display = true,
                        title = new { display = true, text = "Date/Heure" }
                    },
                    y = new
                    {
                        display = true,
                        title = new { display = true, text = data.DatasetLabel }
                    }
                }
            }
        };
    }

    private async Task<Stream?> FetchChartImageAsync(object chartConfig, CancellationToken ct)
    {
        var httpClient = httpClientFactory.CreateClient();
        
        var requestBody = new
        {
            chart = chartConfig,
            width = 800,
            height = 400,
            backgroundColor = "white",
            format = "png"
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(QuickChartUrl, content, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("QuickChart API returned {StatusCode}", response.StatusCode);
            return null;
        }

        var memoryStream = new MemoryStream();
        await response.Content.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;
        
        return memoryStream;
    }

    private class ChartDataResult
    {
        public List<string> Labels { get; } = [];
        public List<double> Values { get; } = [];
        public string DatasetLabel { get; set; } = "";
        public string BorderColor { get; set; } = "#FF6384";
    }
}
