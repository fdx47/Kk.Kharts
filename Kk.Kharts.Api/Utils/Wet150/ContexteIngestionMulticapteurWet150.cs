using Kk.Kharts.Shared.DTOs;

namespace Kk.Kharts.Api.Utils.Wet150;

internal sealed record ContexteIngestionMulticapteurWet150(
    string Endpoint,
    DateTime HorodatageMesure,
    string HorodatageMesureFormate,
    string ChargeUtileRecue,
    DeviceDto Appareil,
    string DevEui,
    IReadOnlyDictionary<string, string?> ChampsSdi12,
    int NombreChampsValides,
    string DonneesCapteursBrutes);
