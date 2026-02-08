using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.Notifications;

public sealed class EmailNotificationChannel : INotificationChannel
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailNotificationChannel> _logger;

    public EmailNotificationChannel(IEmailService emailService, ILogger<EmailNotificationChannel> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public NotificationChannelType Type => NotificationChannelType.Email;

    public async Task<NotificationChannelResult> SendAsync(User user, AlertNotification notification, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            _logger.LogDebug("Utilisateur {UserId} sans email valide pour notification.", user.Id);
            return NotificationChannelResult.MissingData;
        }

        try
        {
            await _emailService.SendAsync(user.Email, notification.Title, notification.BodyHtml);
            _logger.LogInformation("Notification email envoyée à {UserId} ({Email}).", user.Id, user.Email);
            return NotificationChannelResult.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Échec de l'envoi email pour l'utilisateur {UserId}.", user.Id);
            return NotificationChannelResult.Failed;
        }
    }
}
