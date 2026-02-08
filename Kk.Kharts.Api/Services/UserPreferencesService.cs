using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;
using Kk.Kharts.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Services;

/// <summary>
/// Service applicatif centralisant la gestion des préférences utilisateurs (notification, portée d'accès, Pushover).
/// </summary>
public class UserPreferencesService(AppDbContext context, ILogger<UserPreferencesService> logger) : IUserPreferencesService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<UserPreferencesService> _logger = logger;

    private static readonly PushoverSoundOptionDto[] SoundOptions =
    [
        new() { Id = "pushover", Label = "Pushover", Description = "Son par défaut" },
        new() { Id = "bike", Label = "Bike", Description = "Sonnerie de vélo" },
        new() { Id = "bugle", Label = "Bugle", Description = "Clairon militaire" },
        new() { Id = "cashregister", Label = "Cash Register", Description = "Caisse enregistreuse" },
        new() { Id = "classical", Label = "Classical", Description = "Accord orchestral" },
        new() { Id = "cosmic", Label = "Cosmic", Description = "Bip futuriste" },
        new() { Id = "falling", Label = "Falling", Description = "Effet chute libre" },
        new() { Id = "gamelan", Label = "Gamelan", Description = "Percussion indonésienne" },
        new() { Id = "incoming", Label = "Incoming", Description = "Notification entrante" },
        new() { Id = "intermission", Label = "Intermission", Description = "Intervalle musical" },
        new() { Id = "magic", Label = "Magic", Description = "Petit carillon" },
        new() { Id = "mechanical", Label = "Mechanical", Description = "Cliquetis mécanique" },
        new() { Id = "pianobar", Label = "Piano Bar", Description = "Accord de piano" },
        new() { Id = "siren", Label = "Siren", Description = "Alerte stridente" },
        new() { Id = "spacealarm", Label = "Space Alarm", Description = "Alerte spatiale" },
        new() { Id = "tugboat", Label = "Tug Boat", Description = "Corne de bateau" },
        new() { Id = "alien", Label = "Alien Alarm", Description = "Effet science-fiction" },
        new() { Id = "climb", Label = "Climb", Description = "Progression ascendante" },
        new() { Id = "persistent", Label = "Persistent", Description = "Répétition insistante" },
        new() { Id = "echo", Label = "Echo", Description = "Réverbération" },
        new() { Id = "updown", Label = "Up Down", Description = "Montée / descente" },
        new() { Id = "vibrate", Label = "Vibrate", Description = "Vibration uniquement" },
        new() { Id = "none", Label = "None", Description = "Pas de son" },
        new() { Id = "default", Label = "Device Default", Description = "Laisser le device choisir" }
    ];

    private static readonly PushoverPriorityOptionDto[] PriorityOptions =
    [
        new() { Value = -2, Label = "Lowest", Description = "Livré en arrière-plan sans notification", RequiresRetryAndExpire = false },
        new() { Value = -1, Label = "Low", Description = "Ajouté à l'historique sans son", RequiresRetryAndExpire = false },
        new() { Value = 0, Label = "Normal", Description = "Comportement standard", RequiresRetryAndExpire = false },
        new() { Value = 1, Label = "High", Description = "Ignorer DND/volume", RequiresRetryAndExpire = false },
        new() { Value = 2, Label = "Emergency", Description = "Répéter jusqu'à acquittement", RequiresRetryAndExpire = true }
    ];

    public async Task<UserPreferencesDto?> GetAsync(int userId, CancellationToken ct = default)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Pushover)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        return user?.ToDto();
    }

    public Task<PushoverMetadataDto> GetPushoverMetadataAsync(CancellationToken ct = default)
    {
        var dto = new PushoverMetadataDto
        {
            Sounds = SoundOptions,
            Priorities = PriorityOptions,
            MinRetrySeconds = 30,
            MaxExpireSeconds = 10800
        };

        return Task.FromResult(dto);
    }

    public async Task<UserPreferencesDto> UpdateNotificationPreferenceAsync(int userId, UserNotificationPreferenceUpdateDto dto, CancellationToken ct = default)
    {
        var user = await LoadUserAsync(userId, ct);

        user.NotificationPreference = dto.Preference;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Préférence de notification mise à jour pour {UserId} ({Preference})", user.Id, user.NotificationPreference);

        return user.ToDto();
    }

    public async Task<UserPreferencesDto> UpdateAccessScopeAsync(int userId, UserAccessScopeUpdateDto dto, CancellationToken ct = default)
    {
        var user = await LoadUserAsync(userId, ct);

        user.AccessLevel = dto.AccessLevel;

        if (dto.CompanyId.HasValue && dto.CompanyId.Value != user.CompanyId)
        {
            var companyExists = await _context.Companies.AnyAsync(c => c.Id == dto.CompanyId.Value, ct);
            if (!companyExists)
            {
                throw new InvalidOperationException($"La société {dto.CompanyId} n'existe pas.");
            }

            user.CompanyId = dto.CompanyId.Value;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Portée d'accès mise à jour pour {UserId} (Level={AccessLevel}, Company={CompanyId})", user.Id, user.AccessLevel, user.CompanyId);

        return user.ToDto();
    }

    public async Task<UserPreferencesDto> UpdatePushoverSettingsAsync(int userId, UserPushoverSettingsUpdateDto dto, CancellationToken ct = default)
    {
        var user = await LoadUserAsync(userId, ct);

        if (dto.DisablePushover)
        {
            user.Pushover = null;
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Paramètres Pushover supprimés pour {UserId}", user.Id);
            return user.ToDto();
        }

        if (string.IsNullOrWhiteSpace(dto.AppToken) || string.IsNullOrWhiteSpace(dto.UserKey))
        {
            throw new InvalidOperationException("AppToken et UserKey sont obligatoires pour activer Pushover.");
        }

        user.Pushover ??= new PushoverSettings();

        user.Pushover.AppToken = dto.AppToken.Trim();
        user.Pushover.UserKey = dto.UserKey.Trim();
        user.Pushover.Sound = NormalizePushoverSound(dto.Sound);
        user.Pushover.Device = NormalizeOptional(dto.Device);
        user.Pushover.Title = NormalizeOptional(dto.Title);
        user.Pushover.MessageTemplate = NormalizeOptional(dto.MessageTemplate);
        user.Pushover.Priority = dto.Priority;
        user.Pushover.RetrySeconds = dto.RetrySeconds;
        user.Pushover.ExpireSeconds = dto.ExpireSeconds;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Paramètres Pushover mis à jour pour {UserId}", user.Id);

        return user.ToDto();
    }

    private async Task<User> LoadUserAsync(int userId, CancellationToken ct)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Pushover)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
        {
            throw new KeyNotFoundException($"Utilisateur {userId} introuvable.");
        }

        return user;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizePushoverSound(string? sound)
    {
        var trimmed = NormalizeOptional(sound);
        return trimmed?.ToLowerInvariant();
    }
}
