namespace Kk.Kharts.Api.Services.Notifications;

/// <summary>
/// Représente un document d'alerte métier indépendant du canal de diffusion.
/// </summary>
public sealed class AlertNotification
{
    public AlertNotification(
        string title,
        string bodyHtml,
        string bodyText,
        DateTimeOffset occurredAt,
        IReadOnlyList<NotificationActionRow>? actions = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Le titre est obligatoire.", nameof(title));

        if (string.IsNullOrWhiteSpace(bodyHtml))
            throw new ArgumentException("Le corps HTML est obligatoire.", nameof(bodyHtml));

        if (string.IsNullOrWhiteSpace(bodyText))
            throw new ArgumentException("Le corps texte est obligatoire.", nameof(bodyText));

        Title = title;
        BodyHtml = bodyHtml;
        BodyText = bodyText;
        OccurredAt = occurredAt;
        Actions = actions ?? Array.Empty<NotificationActionRow>();
    }

    public string Title { get; }
    public string BodyHtml { get; }
    public string BodyText { get; }
    public DateTimeOffset OccurredAt { get; }
    public IReadOnlyList<NotificationActionRow> Actions { get; }
}

public sealed record NotificationActionRow(IReadOnlyList<NotificationAction> Actions)
{
    public static NotificationActionRow FromActions(params NotificationAction[] actions)
        => new(actions);
}

public sealed record NotificationAction(
    string Label,
    string? CallbackData = null,
    string? Url = null)
{
    public bool IsCallback => !string.IsNullOrWhiteSpace(CallbackData);
    public bool IsUrl => !string.IsNullOrWhiteSpace(Url);
}
