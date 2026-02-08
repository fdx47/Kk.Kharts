using Kk.Kharts.Shared.DTOs;
using System.Globalization;

namespace Kk.Kharts.Maui.Models;

/// <summary>
/// Device model for UI display with additional computed properties.
/// </summary>
public sealed class DeviceModel
{
    public int Id { get; init; }
    public string DevEui { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? InstallationLocation { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public float Battery { get; init; }
    public int? Model { get; init; }
    public string LastSendAt { get; init; } = "??";
    public string LastTransmissionLocal { get; init; } = "—";
    public bool? ActiveInKropKontrol { get; init; }

    public DeviceType DeviceType => Model switch
    {
        2 => DeviceType.Em300Di,
        7 => DeviceType.Em300Th,
        47 => DeviceType.Uc502Wet150,
        61 => DeviceType.Uc502Modbus,
        62 => DeviceType.Uc502MultiSensor,
        _ => DeviceType.Unknown
    };

    public string DeviceTypeName => DeviceType switch
    {
        DeviceType.Em300Di => "EM300-DI (Compteur)",
        DeviceType.Em300Th => "EM300-TH (Temp/Hum)",
        DeviceType.Uc502Wet150 => "UC502 WET150 (Sol)",
        DeviceType.Uc502Modbus => "UC502 Modbus",
        DeviceType.Uc502MultiSensor => "UC502 Multi-Sonde",
        _ => "Inconnu"
    };

    public string BatteryDisplay => $"{Math.Round(Battery)}%";

    public Color BatteryColor => Battery switch
    {
        >= 20f => Colors.Green,
        >= 10f => Colors.Orange,
        _ => Colors.Red
    };

    public static DeviceModel FromDto(DeviceDto dto) => new()
    {
        Id = dto.Id,
        DevEui = dto.DevEui,
        Name = dto.Name,
        Description = dto.Description,
        InstallationLocation = dto.InstallationLocation,
        CompanyName = dto.CompanyName,
        Battery = dto.Battery,
        Model = dto.Model,
        LastSendAt = dto.LastSendAt,
        LastTransmissionLocal = FormatLocalTimestamp(dto.LastSendAt),
        ActiveInKropKontrol = dto.ActiveInKropKontrol
    };

    private static string FormatLocalTimestamp(string lastSendAt)
    {
        if (DateTime.TryParse(lastSendAt,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var utc))
        {
            var local = utc.ToLocalTime();
            return local.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture);
        }

        return lastSendAt;
    }
}

public enum DeviceType
{
    Unknown = 0,
    Em300Di = 2,
    Em300Th = 7,
    Uc502Wet150 = 47,
    Uc502Modbus = 61,
    Uc502MultiSensor = 62
}
