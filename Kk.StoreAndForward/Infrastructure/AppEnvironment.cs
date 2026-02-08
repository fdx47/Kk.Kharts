namespace KK.UG6x.StoreAndForward.Infrastructure;

public static class AppEnvironment
{
    public static AppEnvironmentPaths Initialize()
    {
        var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        var appDataPath = Path.Combine(programData, "kropkontrol", "KK.Milesight", "StoreAndForward");
        Directory.CreateDirectory(appDataPath);

        var dbPath = Path.Combine(appDataPath, "KK.StoreAndForward.db");

        EnsureDatabaseFileExists(dbPath);

        return new AppEnvironmentPaths(appDataPath, dbPath);
    }

    private static void EnsureDatabaseFileExists(string dbPath)
    {
        var dbDirectory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        if (File.Exists(dbPath))
        {
            return;
        }

        try
        {
            using var _ = File.Create(dbPath);
            Console.WriteLine("[Bootstrap] Base SQLite recréée dans AppData.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Bootstrap] Impossible de créer la base locale: {ex.Message}");
        }
    }
}

public record AppEnvironmentPaths(string AppDataPath, string DbPath);
