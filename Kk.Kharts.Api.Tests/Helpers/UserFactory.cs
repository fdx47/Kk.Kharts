using Kk.Kharts.Shared.Entities;
using Kk.Kharts.Shared.Enums;

namespace Kk.Kharts.Api.Tests.Helpers;

/// <summary>
/// Factory pour créer des entités User complètes avec toutes les configurations de notification.
/// Utilisé pour tester le système de notification (Telegram, Pushover, Email).
/// </summary>
public static class UserFactory
{
    /// <summary>
    /// Crée l'utilisateur cesar@blazor.com avec tous les canaux de notification configurés.
    /// Cet utilisateur est utilisé pour valider l'envoi des alertes.
    /// </summary>
    public static User CreateCesarUser(NotificationChannelPreference preference = NotificationChannelPreference.Tous)
    {
        var company = new Company
        {
            Id = 1,
            Name = "Blazor Corp",
            HeaderNameApiKey = "X-API-Key",
            HeaderValueApiKey = "test-api-key-123"
        };

        return new User
        {
            Id = 42,
            Email = "cesar@blazor.com",
            Nom = "Cesar",
            Password = "hashed_password",
            Role = "Root",
            CompanyId = company.Id,
            Company = company,
            NotificationPreference = preference,
            TelegramUserId = 555_123_456,
            TelegramUsername = "cesar_blazor",
            TelegramLinkedAt = DateTime.UtcNow.AddDays(-30),
            Pushover = new PushoverSettings
            {
                AppToken = "a1b2c3d4e5f6g7h8i9j0",
                UserKey = "u1v2w3x4y5z6a7b8c9d0",
                Sound = "pushover",
                Device = "cesar-phone",
                Title = "Alerte Kharts",
                Priority = 0
            },
            AccessLevel = UserAccessLevel.CompanyAndSubsidiaries,
            SignupDate = DateTime.UtcNow.AddYears(-1)
        };
    }

    /// <summary>
    /// Crée un utilisateur avec uniquement Telegram configuré.
    /// </summary>
    public static User CreateTelegramOnlyUser()
    {
        var user = CreateCesarUser(NotificationChannelPreference.Telegram);
        user.Pushover = null;
        user.Email = "telegram-only@blazor.com";
        return user;
    }

    /// <summary>
    /// Crée un utilisateur avec uniquement Pushover configuré.
    /// </summary>
    public static User CreatePushoverOnlyUser()
    {
        var user = CreateCesarUser(NotificationChannelPreference.Pushover);
        user.TelegramUserId = null;
        user.TelegramUsername = null;
        user.Email = "pushover-only@blazor.com";
        return user;
    }

    /// <summary>
    /// Crée un utilisateur avec uniquement Email configuré.
    /// </summary>
    public static User CreateEmailOnlyUser()
    {
        var user = CreateCesarUser(NotificationChannelPreference.Email);
        user.TelegramUserId = null;
        user.Pushover = null;
        return user;
    }

    /// <summary>
    /// Crée un utilisateur sans aucune notification configurée.
    /// </summary>
    public static User CreateUserWithoutNotifications()
    {
        var user = CreateCesarUser(NotificationChannelPreference.Aucun);
        user.TelegramUserId = null;
        user.Pushover = null;
        return user;
    }

    /// <summary>
    /// Crée un utilisateur avec Pushover incomplet (sans clés).
    /// Utilisé pour tester les scénarios d'erreur.
    /// </summary>
    public static User CreateUserWithIncompletePushover()
    {
        var user = CreateCesarUser(NotificationChannelPreference.Pushover);
        user.Pushover = new PushoverSettings
        {
            AppToken = null,
            UserKey = null
        };
        return user;
    }
}
