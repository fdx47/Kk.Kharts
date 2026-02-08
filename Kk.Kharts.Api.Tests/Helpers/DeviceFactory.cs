using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Tests.Helpers;

/// <summary>
/// Factory pour créer des entités Device pour les tests.
/// Permet de générer des devices avec différentes configurations.
/// </summary>
public static class DeviceFactory
{
    /// <summary>
    /// Crée un device standard pour les tests UC502.
    /// </summary>
    public static Device CreateUc502Device(string devEui = "24E1249999999999", int companyId = 1)
    {
        return new Device
        {
            Id = 1,
            DevEui = devEui,
            Name = "UC502 Test Device",
            Description = "Device de test pour UC502",
            InstallationLocation = "Serre A",
            Battery = 85.5f,
            LastSendAt = DateTime.UtcNow.AddMinutes(-5).ToString("yyyy-MM-ddTHH:mm:ssZ"),
            LastSeenAt = DateTime.UtcNow.AddMinutes(-5),
            CompanyId = companyId
        };
    }

    /// <summary>
    /// Crée un device standard pour les tests EM300-TH.
    /// </summary>
    public static Device CreateEm300ThDevice(string devEui = "24E1240000000001", int companyId = 1)
    {
        return new Device
        {
            Id = 2,
            DevEui = devEui,
            Name = "EM300-TH Test Device",
            Description = "Capteur température/humidité",
            InstallationLocation = "Serre B",
            Battery = 92.0f,
            LastSendAt = DateTime.UtcNow.AddMinutes(-10).ToString("yyyy-MM-ddTHH:mm:ssZ"),
            LastSeenAt = DateTime.UtcNow.AddMinutes(-10),
            CompanyId = companyId
        };
    }

    /// <summary>
    /// Crée un device standard pour les tests EM300-DI.
    /// </summary>
    public static Device CreateEm300DiDevice(string devEui = "24E1240000000002", int companyId = 1)
    {
        return new Device
        {
            Id = 3,
            DevEui = devEui,
            Name = "EM300-DI Test Device",
            Description = "Capteur entrées digitales",
            InstallationLocation = "Serre C",
            Battery = 78.0f,
            LastSendAt = DateTime.UtcNow.AddMinutes(-15).ToString("yyyy-MM-ddTHH:mm:ssZ"),
            LastSeenAt = DateTime.UtcNow.AddMinutes(-15),
            CompanyId = companyId
        };
    }

    /// <summary>
    /// Crée un device hors ligne (pour tester les notifications).
    /// </summary>
    public static Device CreateOfflineDevice(string devEui = "24E1248888888888", int companyId = 1)
    {
        return new Device
        {
            Id = 4,
            DevEui = devEui,
            Name = "Device Offline",
            Description = "Device hors ligne pour tests",
            InstallationLocation = "Serre D",
            Battery = 45.0f,
            LastSendAt = DateTime.UtcNow.AddHours(-3).ToString("yyyy-MM-ddTHH:mm:ssZ"),
            LastSeenAt = DateTime.UtcNow.AddHours(-3),
            CompanyId = companyId
        };
    }
}
