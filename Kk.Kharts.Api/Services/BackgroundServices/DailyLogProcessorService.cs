using Kk.Kharts.Api.Utility.Constants;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Kk.Kharts.Api.Services.BackgroundServices
{
    public class DailyLogProcessorService : BackgroundService
    {
        private readonly string _logDir = Path.Combine(AppContext.BaseDirectory, GlobalConstants.LogsDirectoryName);
        private readonly string _summaryFilePath = Path.Combine(AppContext.BaseDirectory, GlobalConstants.LogsDirectoryName, "totals.txt");
        private readonly ILogger<DailyLogProcessorService> _logger;

        public DailyLogProcessorService(ILogger<DailyLogProcessorService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_summaryFilePath)!);

            // Executa imediatamente ao iniciar
            //try
            //{
            //    await ProcessPreviousDayLogAsync();
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Erreur lors du traitement du journal quotidien");
            //}

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                // Próxima execução às 2h da manhã UTC
                var nextRun = DateTime.Today.AddDays(now.Hour >= 2 ? 1 : 0).AddHours(2);

                var delay = nextRun - now;

                if (delay.TotalMilliseconds <= 0)
                    delay = TimeSpan.FromSeconds(10); // fallback

                await Task.Delay(delay, stoppingToken);

                try
                {
                    await ProcessPreviousDayLogAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors du traitement du journal quotidien");
                }
            }
        }

        private async Task ProcessPreviousDayLogAsync()
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var fileName = $"{yesterday:ddMMyy}.txt";
            var filePath = Path.Combine(_logDir, fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogInformation("Fichier du jour précédent non trouvé : {FilePath}", filePath);
                return;
            }

            var lines = await File.ReadAllLinesAsync(filePath);

            // Dicionário para guardar dados do dia anterior
            var dailyCounts = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

            string? currentUser = null;
            string? currentMethod = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("👤"))
                {
                    currentUser = line.Split(':', 2).Last().Trim();
                }
                else if (line.StartsWith("📥"))
                {
                    currentMethod = line.Substring(2).Trim().ToUpper();

                    if (!string.IsNullOrEmpty(currentUser) && !string.IsNullOrEmpty(currentMethod))
                    {
                        if (!dailyCounts.ContainsKey(currentUser))
                            dailyCounts[currentUser] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                        if (!dailyCounts[currentUser].ContainsKey(currentMethod))
                            dailyCounts[currentUser][currentMethod] = 0;

                        dailyCounts[currentUser][currentMethod]++;
                    }
                }
            }

            // Ler os totais atuais do ficheiro (se existir)
            var totalCounts = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

            if (File.Exists(_summaryFilePath))
            {
                var totalLines = await File.ReadAllLinesAsync(_summaryFilePath);
                string? user = null;

                foreach (var line in totalLines)
                {
                    if (line.StartsWith("Utilisateur:"))
                    {
                        user = line.Substring("Utilisateur:".Length).Trim();
                        if (!totalCounts.ContainsKey(user))
                            totalCounts[user] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    }
                    else if (!string.IsNullOrWhiteSpace(line) && user != null)
                    {
                        // Exemplo linha: "  GET: 10"
                        var parts = line.Trim().Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int count))
                        {
                            var method = parts[0];
                            if (!totalCounts[user].ContainsKey(method))
                                totalCounts[user][method] = 0;

                            totalCounts[user][method] = count;
                        }
                    }
                }
            }

            // Atualizar os totais com os dados do dia
            foreach (var userEntry in dailyCounts)
            {
                if (!totalCounts.ContainsKey(userEntry.Key))
                    totalCounts[userEntry.Key] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var methodEntry in userEntry.Value)
                {
                    if (!totalCounts[userEntry.Key].ContainsKey(methodEntry.Key))
                        totalCounts[userEntry.Key][methodEntry.Key] = 0;

                    totalCounts[userEntry.Key][methodEntry.Key] += methodEntry.Value;
                }
            }

            // Gravar os totais atualizados no ficheiro
            var sb = new StringBuilder();
            sb.AppendLine("=== Totaux cumulés ===\n");

            foreach (var user in totalCounts)
            {
                sb.AppendLine($"Utilisateur : {user.Key}");
                foreach (var method in user.Value)
                {
                    sb.AppendLine($"  {method.Key}: {method.Value}");
                }
                sb.AppendLine();
            }

            await File.WriteAllTextAsync(_summaryFilePath, sb.ToString());

            _logger.LogInformation("Fichier des totaux mis à jour : {FilePath}", _summaryFilePath);
        }
    }

}