using Kk.Kharts.Api.Data;
using Kk.Kharts.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Handler para o comando /status - Estado geral do sistema.
/// </summary>
public sealed class StatusCommandHandler(
    IServiceScopeFactory scopeFactory,
    ITelegramService telegram) : ITelegramCommandHandler
{
    public string Command => TelegramConstants.Commands.Status;
    public string Description => "Résumé rapide de la santé du système";


    public async Task HandleAsync(Message message, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;
        var oneHourAgo = now.AddHours(-1);
        var twentyMinutesAgo = now.AddMinutes(-20);

        // Estatísticas gerais
        var totalDevices = await db.Devices.CountAsync(d => d.ActiveInKropKontrol, ct);
        var onlineDevices = await db.Devices.CountAsync(d => d.ActiveInKropKontrol && d.LastSeenAt >= twentyMinutesAgo, ct);
        var offlineDevices = await db.Devices.CountAsync(d => d.ActiveInKropKontrol && d.HasCommunicationAlarm, ct);
        var lowBatteryDevices = await db.Devices.CountAsync(d => d.ActiveInKropKontrol && d.Battery <= 20, ct);

        // Alertas ativos
        var activeAlarms = await db.AlarmRules.CountAsync(a => a.Enabled && a.IsAlarmActive, ct);

        // Empresas
        var totalCompanies = await db.Companies.CountAsync(ct);

        // Últimas leituras
        var lastReading = await db.Uc502Wet150s
            .OrderByDescending(r => r.Timestamp)
            .Select(r => r.Timestamp)
            .FirstOrDefaultAsync(ct);

        var lastReadingText = lastReading != default
            ? FormatTimeAgo(lastReading)
            : "Aucune donnée";

        // Calcular percentagens
        var onlinePercent = totalDevices > 0 ? (onlineDevices * 100.0 / totalDevices) : 0;
        var healthEmoji = onlinePercent switch
        {
            >= 90 => TelegramConstants.Emojis.Success,
            >= 70 => TelegramConstants.Emojis.Warning,
            _ => TelegramConstants.Emojis.Error
        };

        var response = $"""
            {TelegramConstants.Emojis.Chart} <b>État du Système KropKontrol</b>
            
            {healthEmoji} <b>Santé Générale:</b> {onlinePercent:F0}% en ligne
            
            ━━━━━━━━━━━━━━━━━━━━━━
            
            {TelegramConstants.Emojis.Device} <b>Capteurs</b>
            • Total: <b>{totalDevices}</b>
            • {TelegramConstants.Emojis.Success} En ligne: <b>{onlineDevices}</b>
            • {TelegramConstants.Emojis.Warning} Hors ligne: <b>{offlineDevices}</b>
            • 🪫 Batterie faible: <b>{lowBatteryDevices}</b>
            
            {TelegramConstants.Emojis.Bell} <b>Alertes</b>
            • Actives: <b>{activeAlarms}</b>
            
            {TelegramConstants.Emojis.Farm} <b>Entreprises</b>
            • Total: <b>{totalCompanies}</b>
            
            {TelegramConstants.Emojis.Clock} <b>Dernière Lecture</b>
            • {lastReadingText}
            
            ━━━━━━━━━━━━━━━━━━━━━━
            
            {TelegramConstants.Emojis.Calendar} <i>Mis à jour: {now:dd/MM/yyyy HH:mm} UTC</i>
            """;

        await telegram.SendToChatAsync(
            message.Chat.Id,
            response,
            ParseMode.Html,
            ct: ct);
    }

    private static string FormatTimeAgo(DateTime dateTime)
    {
        var ts = DateTime.UtcNow - dateTime;

        if (ts.TotalMinutes < 1) return $"Il y a {ts.Seconds} secondes";
        if (ts.TotalMinutes < 2) return "Il y a 1 minute";
        if (ts.TotalHours < 1) return $"Il y a {(int)ts.TotalMinutes} minutes";
        if (ts.TotalHours < 24) return $"Il y a {(int)ts.TotalHours}h {ts.Minutes:D2}m";
        if (ts.TotalDays < 2) return "Il y a 1 jour";
        if (ts.TotalDays < 30) return $"Il y a {(int)ts.TotalDays} jours";
        if (ts.TotalDays < 60) return "Il y a 1 mois";
        if (ts.TotalDays < 365) return $"Il y a {(int)(ts.TotalDays / 30)} mois";
        if (ts.TotalDays < 730) return "Il y a 1 an";
        return $"Il y a {(int)(ts.TotalDays / 365)} ans";
    }
}
