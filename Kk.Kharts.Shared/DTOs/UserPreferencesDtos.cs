using System.ComponentModel.DataAnnotations;
using Kk.Kharts.Shared.Entities;
using Kk.Kharts.Shared.Enums;

namespace Kk.Kharts.Shared.DTOs;

/// <summary>
/// Représente l'état complet des préférences d'un utilisateur pour l'UI admin/support.
/// </summary>
public class UserPreferencesDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public NotificationChannelPreference NotificationPreference { get; set; }
    public UserAccessLevel AccessLevel { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public PushoverSettingsDto? Pushover { get; set; }
}

/// <summary>
/// Préférences Pushover prêtes à renvoyer vers le front en lecture seule.
/// </summary>
public class PushoverSettingsDto
{
    public string? AppToken { get; set; }
    public string? UserKey { get; set; }
    public string? Sound { get; set; }
    public string? Device { get; set; }
    public string? Title { get; set; }
    public string? MessageTemplate { get; set; }
    public int? Priority { get; set; }
    public int? RetrySeconds { get; set; }
    public int? ExpireSeconds { get; set; }
}

/// <summary>
/// Métadonnées exposées au front pour afficher les options Pushover.
/// </summary>
public class PushoverMetadataDto
{
    public IReadOnlyList<PushoverSoundOptionDto> Sounds { get; init; } = Array.Empty<PushoverSoundOptionDto>();
    public IReadOnlyList<PushoverPriorityOptionDto> Priorities { get; init; } = Array.Empty<PushoverPriorityOptionDto>();
    public int MinRetrySeconds { get; init; }
    public int MaxExpireSeconds { get; init; }
}

public class PushoverSoundOptionDto
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public string? Description { get; init; }
}

public class PushoverPriorityOptionDto
{
    public int Value { get; init; }
    public required string Label { get; init; }
    public string? Description { get; init; }
    public bool RequiresRetryAndExpire { get; init; }
}

/// <summary>
/// Requête pour modifier le canal de notification préféré d'un utilisateur.
/// </summary>
public class UserNotificationPreferenceUpdateDto
{
    [Required]
    /// <summary>
    /// Canal(is) desejados. Valores aceitos: Telegram, Pushover, Email,
    /// TelegramEtPushover, TelegramEtEmail, PushoverEtEmail ou Tous (os três simultaneamente).
    /// </summary>
    public NotificationChannelPreference Preference { get; set; }
}

/// <summary>
/// Requête pour ajuster la portée des notifications (société / filiales) d'un utilisateur.
/// </summary>
public class UserAccessScopeUpdateDto
{
    [Required]
    public UserAccessLevel AccessLevel { get; set; }

    /// <summary>
    /// Permet de rattacher l'utilisateur à une autre entreprise lors d'une mise à jour d'accès.
    /// </summary>
    public int? CompanyId { get; set; }
}

/// <summary>
/// Requête pour mettre à jour les paramètres Pushover d'un utilisateur.
/// </summary>
public class UserPushoverSettingsUpdateDto
{
    /// <summary>
    /// Lorsque vrai, supprime entièrement les réglages Pushover (et désactive l'usage).
    /// </summary>
    public bool DisablePushover { get; set; }

    public string? AppToken { get; set; }
    public string? UserKey { get; set; }
    public string? Sound { get; set; }
    public string? Device { get; set; }
    public string? Title { get; set; }
    public string? MessageTemplate { get; set; }
    public int? Priority { get; set; }
    public int? RetrySeconds { get; set; }
    public int? ExpireSeconds { get; set; }
}

public static class UserPreferencesMappingExtensions
{
    public static UserPreferencesDto ToDto(this User user)
    {
        return new UserPreferencesDto
        {
            UserId = user.Id,
            Email = user.Email,
            Nom = user.Nom,
            NotificationPreference = user.NotificationPreference,
            AccessLevel = user.AccessLevel,
            CompanyId = user.CompanyId,
            CompanyName = user.Company?.Name ?? string.Empty,
            Pushover = user.Pushover is null
                ? null
                : new PushoverSettingsDto
                {
                    AppToken = user.Pushover.AppToken,
                    UserKey = user.Pushover.UserKey,
                    Sound = user.Pushover.Sound,
                    Device = user.Pushover.Device,
                    Title = user.Pushover.Title,
                    MessageTemplate = user.Pushover.MessageTemplate,
                    Priority = user.Pushover.Priority,
                    RetrySeconds = user.Pushover.RetrySeconds,
                    ExpireSeconds = user.Pushover.ExpireSeconds
                }
        };
    }
}
