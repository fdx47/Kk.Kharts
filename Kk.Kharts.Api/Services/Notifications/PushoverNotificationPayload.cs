namespace Kk.Kharts.Api.Services.Notifications;

public sealed class PushoverNotificationPayload
{
    public PushoverNotificationPayload(
        string title,
        string message,
        string? sound = null,
        string? device = null,
        int? priority = null,
        int? retrySeconds = null,
        int? expireSeconds = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Le titre Pushover est obligatoire.", nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Le message Pushover est obligatoire.", nameof(message));

        Title = title;
        Message = message;
        Sound = string.IsNullOrWhiteSpace(sound) ? null : sound.Trim();
        Device = string.IsNullOrWhiteSpace(device) ? null : device.Trim();
        Priority = priority;
        RetrySeconds = retrySeconds;
        ExpireSeconds = expireSeconds;
    }

    public string Title { get; }
    public string Message { get; }
    public string? Sound { get; }
    public string? Device { get; }
    public int? Priority { get; }
    public int? RetrySeconds { get; }
    public int? ExpireSeconds { get; }
}
