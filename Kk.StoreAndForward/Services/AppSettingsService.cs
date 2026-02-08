using KK.UG6x.StoreAndForward.Domain.Entities;
using KK.UG6x.StoreAndForward.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace KK.UG6x.StoreAndForward.Services;

public class AppSettingsService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<AppSettingsService> _logger;
    private const string EncryptionKey = "KK_Milesight_2026!";

    public AppSettingsService(IDbContextFactory<AppDbContext> contextFactory, ILogger<AppSettingsService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<List<AppSetting>> GetAllSettingsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var settings = await context.AppSettings.ToListAsync();
        
        foreach (var setting in settings.Where(s => s.IsEncrypted))
        {
            setting.Value = DecryptValue(setting.Value);
        }
        
        return settings;
    }

    public async Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var settings = await context.AppSettings
            .Where(s => s.Category == category)
            .ToListAsync();

        var result = new Dictionary<string, string>();
        foreach (var setting in settings)
        {
            result[setting.Key] = setting.IsEncrypted ? DecryptValue(setting.Value) : setting.Value;
        }
        return result;
    }

    public async Task<string?> GetSettingAsync(string key, string category)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var setting = await context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == key && s.Category == category);

        if (setting == null) return null;
        return setting.IsEncrypted ? DecryptValue(setting.Value) : setting.Value;
    }

    public async Task SaveSettingAsync(string key, string value, string category, bool encrypt = false)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == key && s.Category == category);

        var valueToStore = encrypt ? EncryptValue(value) : value;

        if (existing != null)
        {
            existing.Value = valueToStore;
            existing.IsEncrypted = encrypt;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            context.AppSettings.Add(new AppSetting
            {
                Key = key,
                Value = valueToStore,
                Category = category,
                IsEncrypted = encrypt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        _logger.LogInformation($"[AppSettings] Paramètre sauvegardé {category}:{key}");
    }

    public async Task SaveAllSettingsAsync(Dictionary<string, SettingValue> settings)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        foreach (var kvp in settings)
        {
            var parts = kvp.Key.Split(':');
            var category = parts.Length > 1 ? parts[0] : "General";
            var key = parts.Length > 1 ? parts[1] : parts[0];

            var existing = await context.AppSettings
                .FirstOrDefaultAsync(s => s.Key == key && s.Category == category);

            var valueToStore = kvp.Value.Encrypt ? EncryptValue(kvp.Value.Value) : kvp.Value.Value;

            if (existing != null)
            {
                existing.Value = valueToStore;
                existing.IsEncrypted = kvp.Value.Encrypt;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                context.AppSettings.Add(new AppSetting
                {
                    Key = key,
                    Value = valueToStore,
                    Category = category,
                    IsEncrypted = kvp.Value.Encrypt,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();
        _logger.LogInformation($"[AppSettings] {settings.Count} paramètres sauvegardés");
    }

    private static readonly Dictionary<string, (string Key, string Value, string Category, bool Encrypted)> DefaultSettings = new()
    {
        { "GatewayUsername", ("Username", "", "GatewaySettings", false) },
        { "GatewayPassword", ("Password", "", "GatewaySettings", true) },
        { "WorkerInterval", ("IntervalSeconds", "60", "WorkerSettings", false) },
        { "WorkerCheckHost", ("InternetCheckHost", "8.8.8.8", "WorkerSettings", false) },
        { "WorkerForceOffline", ("ForceOffline", "false", "WorkerSettings", false) },
        // { "WorkerOfflineOnlyMode", ("OfflineOnlyMode", "false", "WorkerSettings", false) }
        { "WorkerOfflineOnlyMode", ("OfflineOnlyMode", "true", "WorkerSettings", false) }
    };

    public async Task InitializeDefaultsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        try
        {
            EnsureAppSettingsTableExists(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppSettings] Erreur lors de la vérification de l'existence de la table AppSettings");
        }

        try
        {
            var existingCount = await context.AppSettings.CountAsync();
            if (existingCount > 0)
            {
                _logger.LogInformation("[AppSettings] Les paramètres existent déjà dans la base de données, initialisation ignorée.");
                return;
            }
        }
        catch
        {
            _logger.LogWarning("[AppSettings] Impossible de compter les paramètres existants, tentative d'initialisation.");
        }

        foreach (var (_, tuple) in DefaultSettings)
        {
            await SaveSettingAsync(tuple.Key, tuple.Value, tuple.Category, tuple.Encrypted);
        }

        _logger.LogInformation("[AppSettings] Paramètres initialisés depuis les valeurs par défaut.");
    }

    private string EncryptValue(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        try
        {
            using var aes = Aes.Create();
            var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(EncryptionKey));
            aes.Key = keyBytes[..32];
            aes.IV = keyBytes[..16];
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chiffrement de la valeur");
            return plainText;
        }
    }

    private string DecryptValue(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText)) return encryptedText;

        try
        {
            using var aes = Aes.Create();
            var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(EncryptionKey));
            aes.Key = keyBytes[..32];
            aes.IV = keyBytes[..16];
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du déchiffrement de la valeur");
            return encryptedText;
        }
    }

    private void EnsureAppSettingsTableExists(AppDbContext context)
    {
        try
        {
            var tableInfo = context.Database.GetDbConnection().CreateCommand();
            tableInfo.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='AppSettings';";
            context.Database.OpenConnection();
            
            var result = tableInfo.ExecuteScalar();
            if (result == null)
            {
                _logger.LogInformation("[AppSettings] Création de la table AppSettings...");
                context.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS AppSettings (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Key TEXT NOT NULL,
                        Value TEXT NOT NULL,
                        Category TEXT NOT NULL,
                        IsEncrypted INTEGER NOT NULL DEFAULT 0,
                        CreatedAt TEXT NOT NULL,
                        UpdatedAt TEXT NOT NULL
                    );
                    CREATE UNIQUE INDEX IF NOT EXISTS IX_AppSettings_Category_Key ON AppSettings (Category, Key);
                ");
                _logger.LogInformation("[AppSettings] Table AppSettings créée avec succès.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppSettings] Erreur lors de la création de la table AppSettings");
        }
    }
}

public class SettingValue
{
    public string Value { get; set; } = string.Empty;
    public bool Encrypt { get; set; }
}
