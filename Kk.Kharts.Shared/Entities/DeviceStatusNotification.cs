namespace Kk.Kharts.Shared.Entities;

/// <summary>
/// Registra mensagens de status enviadas para o Telegram, permitindo apagar/atualizar.
/// </summary>
public class DeviceStatusNotification
{
    public int Id { get; set; }
    public int? DeviceId { get; set; }
    public string DevEui { get; set; } = string.Empty;
    public int MessageId { get; set; }
    public string Type { get; set; } = string.Empty; // e.g., "offline", "online"
    public DateTime SentAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
