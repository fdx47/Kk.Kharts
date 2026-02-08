using Kk.Kharts.Api.Data;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Services.Telegram;

/// <summary>
/// Service pour gérer la liaison entre les utilisateurs Telegram et les utilisateurs du système.
/// </summary>
public interface ITelegramUserService
{
    /// <summary>
    /// Récupère l'utilisateur lié à un ID Telegram.
    /// </summary>
    Task<User?> GetUserByTelegramIdAsync(long telegramUserId, CancellationToken ct = default);

    /// <summary>
    /// Lie un compte Telegram à un utilisateur existant via email et mot de passe.
    /// </summary>
    Task<(bool Success, string Message, User? User)> LinkAccountAsync(
        long telegramUserId,
        string telegramUsername,
        string email,
        string password,
        CancellationToken ct = default);

    /// <summary>
    /// Délie un compte Telegram d'un utilisateur.
    /// </summary>
    Task<bool> UnlinkAccountAsync(long telegramUserId, CancellationToken ct = default);

    /// <summary>
    /// Vérifie si un utilisateur Telegram est lié à un compte.
    /// </summary>
    Task<bool> IsLinkedAsync(long telegramUserId, CancellationToken ct = default);

    /// <summary>
    /// Récupère les devices accessibles par un utilisateur Telegram.
    /// </summary>
    Task<List<Device>> GetUserDevicesAsync(long telegramUserId, CancellationToken ct = default);

    /// <summary>
    /// Vérifie si un utilisateur Telegram a accès à un device spécifique.
    /// </summary>
    Task<bool> HasAccessToDeviceAsync(long telegramUserId, string devEui, CancellationToken ct = default);
}

public class TelegramUserService(IServiceScopeFactory scopeFactory) : ITelegramUserService
{
    public async Task<User?> GetUserByTelegramIdAsync(long telegramUserId, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId, ct);
    }

    public async Task<(bool Success, string Message, User? User)> LinkAccountAsync(
        long telegramUserId,
        string telegramUsername,
        string email,
        string password,
        CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Vérifier si ce Telegram ID est déjà lié
        var existingLink = await db.Users
            .FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId, ct);

        if (existingLink != null)
        {
            return (false, $"Ce compte Telegram est déjà lié à {existingLink.Email}", null);
        }

        // Trouver l'utilisateur par email
        var user = await db.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), ct);

        if (user == null)
        {
            return (false, "Email non trouvé dans le système", null);
        }

        // Vérifier le mot de passe (utiliser BCrypt comme dans AuthController)
        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return (false, "Mot de passe incorrect", null);
        }

        // Vérifier si cet utilisateur a déjà un autre Telegram lié
        if (user.TelegramUserId.HasValue && user.TelegramUserId != telegramUserId)
        {
            return (false, "Ce compte est déjà lié à un autre Telegram", null);
        }

        // Lier le compte
        user.TelegramUserId = telegramUserId;
        user.TelegramUsername = telegramUsername;
        user.TelegramLinkedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, "Compte lié avec succès!", user);
    }

    public async Task<bool> UnlinkAccountAsync(long telegramUserId, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId, ct);

        if (user == null)
        {
            return false;
        }

        user.TelegramUserId = null;
        user.TelegramUsername = null;
        user.TelegramLinkedAt = null;

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> IsLinkedAsync(long telegramUserId, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Users.AnyAsync(u => u.TelegramUserId == telegramUserId, ct);
    }

    public async Task<List<Device>> GetUserDevicesAsync(long telegramUserId, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await db.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId, ct);

        if (user == null)
        {
            return [];
        }

        // Si l'utilisateur est root, retourner tous les devices
        if (user.Role.Equals("root", StringComparison.OrdinalIgnoreCase))
        {
            return await db.Devices
                .Include(d => d.Company)
                .Include(d => d.ModeloNavegacao)
                .Where(d => d.ActiveInKropKontrol)
                .OrderByDescending(d => d.LastSeenAt)
                .ToListAsync(ct);
        }

        // Sinon, filtrer par company et subsidiaries selon AccessLevel
        var companyIds = await GetAccessibleCompanyIdsAsync(db, user, ct);

        return await db.Devices
            .Include(d => d.Company)
            .Include(d => d.ModeloNavegacao)
            .Where(d => d.ActiveInKropKontrol && companyIds.Contains(d.CompanyId))
            .OrderByDescending(d => d.LastSeenAt)
            .ToListAsync(ct);
    }

    public async Task<bool> HasAccessToDeviceAsync(long telegramUserId, string devEui, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await db.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId, ct);

        if (user == null)
        {
            return false;
        }

        // Root a accès à tout
        if (user.Role.Equals("root", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var device = await db.Devices
            .FirstOrDefaultAsync(d => d.DevEui == devEui, ct);

        if (device == null)
        {
            return false;
        }

        var companyIds = await GetAccessibleCompanyIdsAsync(db, user, ct);
        return companyIds.Contains(device.CompanyId);
    }

    private static async Task<List<int>> GetAccessibleCompanyIdsAsync(
        AppDbContext db,
        User user,
        CancellationToken ct)
    {
        var companyIds = new List<int> { user.CompanyId };

        // Si l'utilisateur a accès aux subsidiaires
        if (user.AccessLevel == Kk.Kharts.Shared.Enums.UserAccessLevel.CompanyAndSubsidiaries)
        {
            var subsidiaryIds = await db.Companies
                .Where(c => c.ParentCompanyId == user.CompanyId)
                .Select(c => c.Id)
                .ToListAsync(ct);

            companyIds.AddRange(subsidiaryIds);
        }

        return companyIds;
    }
}
