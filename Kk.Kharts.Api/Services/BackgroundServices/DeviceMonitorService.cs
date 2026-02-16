using System.Collections.Generic;
using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Services.BackgroundServices
{
    public class DeviceMonitorService(
        IServiceScopeFactory scopeFactory,
        IKkTimeZoneService timeZoneService,
        ITelegramService telegramService,
        ILogger<DeviceMonitorService> logger) : BackgroundService
    {
        private readonly TimeSpan _deviceTimeout = TimeSpan.FromMinutes(20);
        private readonly IKkTimeZoneService _timeZoneService = timeZoneService;
        private readonly ITelegramService _telegram = telegramService;
        private readonly ILogger<DeviceMonitorService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Primeiro ciclo acontece só no início da próxima hora cheia
            var initialDelay = GetDelayUntilNextFullHour();

            await Task.Delay(initialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Cria um escopo para obter serviços scoped a cada execução do background service
                    using var scope = scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<ITelegramAlarmNotificationService>();

                    var cutoff = DateTime.UtcNow - _deviceTimeout;

                    var removalSummaries = new List<string>();

                    // Busca dispositivos que pararam de comunicar há mais de 20min e ainda não tiveram alarme enviado
                    var inactiveDevices = await dbContext.Devices
                        .Include(d => d.Company)
                        .Where(d => d.ActiveInKropKontrol && d.LastSeenAt < cutoff && !d.HasCommunicationAlarm)
                        .ToListAsync(stoppingToken);


                    foreach (var device in inactiveDevices)
                    {
                        // Converte LastSeenAt para horário de Paris antes de inserir no texto do alerta
                        var parisTime = _timeZoneService.ConvertToParisTime(device.LastSeenAt);

                        var alertMessage = $"""
                            ⚠️ <b>Alerte de périphérique</b>

                            • Nom : {device.Name}
                            • Description : {device.Description}
                            • Entreprise : {device.Company.Name}
                            • DevEui : {device.DevEui}

                            ➡️ Aucune donnée depuis le {parisTime:dd/MM/yy HH:mm:ss} (Paris)
                            """;

                        int? messageId = null;
                        try
                        {
                            messageId = await _telegram.SendToDeviceStatusTopicWithIdAsync(alertMessage, ct: stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Falha ao enviar alerta para o tópico de status do dispositivo {DevEui}", device.DevEui);
                            continue; // pula para o próximo device
                        }

                        if (messageId.HasValue)
                        {
                            // Regista a notificação enviada para poder apagá-la mais tarde
                            var notif = new DeviceStatusNotification
                            {
                                DeviceId = device.Id,
                                DevEui = device.DevEui,
                                MessageId = messageId.Value,
                                Type = "offline",
                                SentAtUtc = DateTime.UtcNow,
                                IsActive = true
                            };
                            dbContext.DeviceStatusNotifications.Add(notif);
                        }

                        device.HasCommunicationAlarm = true;

                        await notificationService.NotifyDeviceOfflineAsync(device, stoppingToken);
                    }

                    // 2. Dispositivos que voltaram a comunicar
                    var recoveredDevices = await dbContext.Devices
                                    .Include(d => d.Company)
                                    .Where(d => d.ActiveInKropKontrol && d.LastSeenAt >= cutoff && d.HasCommunicationAlarm)
                                    .ToListAsync(stoppingToken);


                    foreach (var device in recoveredDevices)
                    {
                        // Limpa o flag para indicar que o device voltou a comunicar
                        device.HasCommunicationAlarm = false;
                        // Converte LastSeenAt para horário de Paris para exibir quando voltou
                        var parisTime = _timeZoneService.ConvertToParisTime(device.LastSeenAt);

                        var recoveryMessage = $"""
                            ✅ <b>Périphérique en ligne</b>

                            • Nom : {device.Name}
                            • Description : {device.Description}
                            • Entreprise : {device.Company.Name}
                            • DevEui : {device.DevEui}

                            ➡️ Communication rétablie le {parisTime:dd/MM/yy HH:mm:ss} (Paris)
                            """;

                        // Apagar alertas anteriores
                        var offlineNotifs = await dbContext.DeviceStatusNotifications
                            .Where(n => n.DevEui == device.DevEui && n.IsActive && n.Type == "offline")
                            .ToListAsync(stoppingToken);

                        foreach (var notif in offlineNotifs)
                        {
                            try
                            {
                                // Remove a mensagem "offline" que estava ativa no tópico do Telegram
                                await _telegram.DeleteFromDeviceStatusTopicAsync(notif.MessageId, stoppingToken);
                                notif.IsActive = false;
                            }
                            catch (Exception ex)
                            {
                                var contexte = $"Suppression Telegram impossible (DevEui: {device.DevEui}, MessageId: {notif.MessageId}, NotificationId: {notif.Id})";
                                _logger.LogWarning(ex, contexte);

                                var detailMessage = $"""
                                    ⚠️ <b>Suppression Telegram échouée</b>

                                    • DevEui : {device.DevEui}
                                    • Nom : {device.Name}
                                    • Entreprise : {device.Company?.Name}
                                    • NotificationId : {notif.Id}
                                    • MessageId : {notif.MessageId}

                                    Raison : {_telegram.EscapeHtml(ex.Message)}
                                    """;

                                await _telegram.SendToDebugTopicAsync(detailMessage, ct: stoppingToken);
                            }
                        }

                        if (offlineNotifs.Count > 0)
                        {
                            removalSummaries.Add($"• {device.Name} ({device.Company.Name}) : {offlineNotifs.Count} message(s)");
                        }

                        var recoveryMessageId = await _telegram.SendToDeviceStatusTopicWithIdAsync(recoveryMessage, ct: stoppingToken);

                        await notificationService.NotifyDeviceOnlineAsync(device, stoppingToken);

                        if (recoveryMessageId.HasValue)
                        {
                            var onlineNotif = new DeviceStatusNotification
                            {
                                DeviceId = device.Id,
                                DevEui = device.DevEui,
                                MessageId = recoveryMessageId.Value,
                                Type = "online",
                                SentAtUtc = DateTime.UtcNow,
                                IsActive = true
                            };
                            dbContext.DeviceStatusNotifications.Add(onlineNotif);

                            // Agenda limpeza automática da mensagem de recuperação após 15 minutos
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);

                                    await _telegram.DeleteFromDeviceStatusTopicAsync(recoveryMessageId.Value, stoppingToken);

                                    using var cleanupScope = scopeFactory.CreateScope();
                                    var cleanupDb = cleanupScope.ServiceProvider.GetRequiredService<AppDbContext>();
                                    var notifToUpdate = await cleanupDb.DeviceStatusNotifications
                                        .FirstOrDefaultAsync(n => n.MessageId == recoveryMessageId.Value, stoppingToken);
                                    if (notifToUpdate is not null)
                                    {
                                        notifToUpdate.IsActive = false;
                                        await cleanupDb.SaveChangesAsync(stoppingToken);
                                    }
                                }
                                catch (TaskCanceledException)
                                {
                                    // ignore cancellation
                                    await _telegram.SendToDebugTopicAsync(" -<DeviceMonitorService>- TaskCanceledException ", ct: stoppingToken);
                                }
                            }, stoppingToken);
                        }
                    }


                    // Persiste todas as alterações feitas neste ciclo
                    await dbContext.SaveChangesAsync(stoppingToken);

                    if (removalSummaries.Count > 0)
                    {
                        var removalMessage = $"""
                            🧹 <b>Nettoyage d'alertes</b>

                            {string.Join("\n", removalSummaries)}
                            """;
                        await _telegram.SendToDebugTopicAsync(removalMessage, ct: stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DeviceMonitorService cycle failed");

                    try
                    {
                        var errorMessage = $"""
                            ❌ <b>DeviceMonitorService</b>
                            Erreur lors de l’évaluation des dispositifs.

                            <code>{_telegram.EscapeHtml(ex.Message)}</code>
                            """;
                        await _telegram.SendToDebugTopicAsync(errorMessage, ct: stoppingToken);
                    }
                    catch (Exception notifyEx)
                    {
                        _logger.LogWarning(notifyEx, "Échec de la notification de l’erreur du DeviceMonitorService sur Telegram");
                    }
                }

                // Aguarda até a próxima hora cheia para executar novamente
                await Task.Delay(GetDelayUntilNextFullHour(), stoppingToken);
            }
        }



        private static TimeSpan GetDelayUntilNextFullHour()
        {
            var now = DateTime.UtcNow;
            var nextHour = now.AddHours(1).Date.AddHours(now.AddHours(1).Hour);
            return nextHour - now;
        }


    }
}
