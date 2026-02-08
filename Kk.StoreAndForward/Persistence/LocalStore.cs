using KK.UG6x.StoreAndForward.Domain.DTOs;
using KK.UG6x.StoreAndForward.Domain.Entities;
using KK.UG6x.StoreAndForward.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace KK.UG6x.StoreAndForward.Persistence;

public class LocalStore : ILocalStore
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<LocalStore> _logger;
    private readonly IConfiguration _configuration;
    private readonly int _checkIntervalSeconds;

    public LocalStore(IDbContextFactory<AppDbContext> contextFactory, ILogger<LocalStore> logger, IConfiguration configuration)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _configuration = configuration;

        // Lê o intervalo de check configurado (padrão: 60s)
        _checkIntervalSeconds = _configuration.GetValue<int>("WorkerSettings:IntervalSeconds", 60);

        using var context = _contextFactory.CreateDbContext();
        context.Database.EnsureCreated();

        // Migration: vérifie si les colonnes existent avant d'essayer de les ajouter
        EnsureColumnExists("IsSent", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumnExists("LastUpdated", "DATETIME");
    }

    private void EnsureColumnExists(string columnName, string columnDefinition)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            // SQLite specific check for column existence
            var tableInfo = context.Database.GetDbConnection().CreateCommand();
            tableInfo.CommandText = "PRAGMA table_info(PendingPayloads);";

            bool columnExists = false;
            context.Database.OpenConnection();
            using (var reader = tableInfo.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"].ToString() == columnName)
                    {
                        columnExists = true;
                        break;
                    }
                }
            }

            if (!columnExists)
            {
                _logger.LogInformation($"[LocalStore] Migration: Ajout de la colonne {columnName}...");
                FormattableString alterCommand = $"ALTER TABLE PendingPayloads ADD COLUMN {columnName} {columnDefinition};";
                context.Database.ExecuteSql(alterCommand);
                _logger.LogInformation($"[LocalStore] Migration: Colonne {columnName} ajoutée avec succès.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[LocalStore] Erreur lors de la vérification/ajout de la colonne {columnName}.");
        }
    }

    public async Task SavePayloadAsync(GatewayPayloadDTO dadosDto, string endpointTipo)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var jsonParaSalvar = JsonSerializer.Serialize(dadosDto);

        // Deduplicação baseada no DevEui e Payload
        var existeDuplicado = await context.PendingPayloads
            .AnyAsync(p => p.DevEui == dadosDto.DevEui && p.PayloadJson == jsonParaSalvar);

        if (existeDuplicado)
        {
            _logger.LogInformation($"[LocalStore] Donnée dupliquée ignorée pour {dadosDto.DevEui}.");
            return;
        }

        var entidadeParaSalvar = new PendingPayload
        {
            DevEui = dadosDto.DevEui,
            PayloadJson = jsonParaSalvar,
            EndpointType = endpointTipo,
            CreatedAt = dadosDto.Timestamp,
            RetryCount = 0
        };

        context.PendingPayloads.Add(entidadeParaSalvar);
        await context.SaveChangesAsync();

        _logger.LogInformation($"[LocalStore] Payload sauvegardé localement pour {dadosDto.DevEui} (Mode Hors Ligne)");
    }

    public async Task<List<PendingPayload>> GetPendingPayloadsAsync(int limiteQtd)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.PendingPayloads
            .Where(p => !p.IsSent)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limiteQtd)
            .ToListAsync();
    }

    public async Task MarkAsSentAsync(long idPayload)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var item = await context.PendingPayloads.FindAsync(idPayload);
        if (item != null)
        {
            item.IsSent = true;
            await context.SaveChangesAsync();
        }
    }

    public async Task<int> GetPendingCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.PendingPayloads.CountAsync(p => !p.IsSent);
    }

    public async Task IncrementRetryCountAsync(long idPayload)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var item = await context.PendingPayloads.FindAsync(idPayload);
        if (item != null)
        {
            item.RetryCount++;
            await context.SaveChangesAsync();
        }
    }


    public async Task<bool> WasAlreadySentAsync(GatewayPayloadDTO dadosDto)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var jsonParaComparar = JsonSerializer.Serialize(dadosDto);

        // Verifica se existe um registro ENVIADO com sucesso (IsSent = true) com o mesmo payload
        var jaEnviado = await context.PendingPayloads
            .AnyAsync(p => p.DevEui == dadosDto.DevEui && p.PayloadJson == jsonParaComparar && p.IsSent);

        return jaEnviado;
    }

    public async Task<bool> AlreadyExistsAsync(GatewayPayloadDTO dadosDto)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var jsonParaComparar = JsonSerializer.Serialize(dadosDto);

        // Otimização: Verifica apenas registos recentes (intervalo configurado + 1 minuto de margem)
        // Só é chamado quando offline ou após recovery, então janela curta é suficiente
        var windowSeconds = _checkIntervalSeconds + 60; // Intervalo + 1 minuto
        var cutoffDate = DateTime.UtcNow.AddSeconds(-windowSeconds);
        
        // Verifica se existe QUALQUER registro (enviado ou pendente) com o mesmo payload
        var jaExiste = await context.PendingPayloads
            .Where(p => p.CreatedAt >= cutoffDate) // Filtra apenas registos recentes
            .AnyAsync(p => p.DevEui == dadosDto.DevEui && p.PayloadJson == jsonParaComparar);

        return jaExiste;
    }

    public async Task CleanupOldRecordsAsync(int daysRetention)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var cutoffDate = DateTime.UtcNow.AddDays(-daysRetention);
            var itemsToRemove = await context.PendingPayloads
                .Where(p => p.IsSent && p.CreatedAt < cutoffDate)
                .ToListAsync();

            if (itemsToRemove.Count > 0)
            {
                _logger.LogInformation($"[Cleanup] Suppression de {itemsToRemove.Count} anciens enregistrements (>{daysRetention} jours).");
                context.PendingPayloads.RemoveRange(itemsToRemove);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du nettoyage des anciens enregistrements.");
        }
    }

    public async Task ClearAllAsync()
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("DELETE FROM PendingPayloads;");
            _logger.LogWarning("[LocalStore] Tous les enregistrements de synchronisation ont été supprimés à la demande de l'utilisateur.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LocalStore] Erreur lors de la suppression des données locales.");
            throw;
        }
    }
}
