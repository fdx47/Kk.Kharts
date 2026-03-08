using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utils;
using Kk.Kharts.Api.Utils.Wet150;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.UC502;
using System.Text.Json;

namespace Kk.Kharts.Api.Services;

public class Wet150MulticapteurService(
    IUc502Repository depotUc502,
    IWet150TramePartielleAggregatorService agregateurTramesPartielles) : IWet150MulticapteurService
{
    private readonly IUc502Repository _depotUc502 = depotUc502;
    private readonly IWet150TramePartielleAggregatorService _agregateurTramesPartielles = agregateurTramesPartielles;

    public async Task TraiterAsync(PayloadWet150MultiSensorFromUg65Dto chargeUtile, DeviceDto appareil, string endpoint)
    {
        var resultatAggregation = await _agregateurTramesPartielles.AgregerAsync(chargeUtile, appareil);
        if (!resultatAggregation.EstPretPourTraitement)
            return;

        var contexte = ConstruireContexte(resultatAggregation.ChargeUtileFusionnee, appareil, endpoint);
        ValiderContexte(contexte);

        var operationPersistance = FabriqueEntitesMulticapteurWet150.CreerOperationPersistance(contexte, _depotUc502);
        await operationPersistance();
    }

    private static ContexteIngestionMulticapteurWet150 ConstruireContexte(PayloadWet150MultiSensorFromUg65Dto chargeUtile, DeviceDto appareil, string endpoint)
    {
        var horodatageMesure = chargeUtile.Timestamp == default ? DateTime.UtcNow : chargeUtile.Timestamp.ToUniversalTime();
        chargeUtile.Timestamp = horodatageMesure;

        var champsSdi12 = chargeUtile.ExtraFieldsSdi12
            .Where(champ => champ.Key.StartsWith("sdi12_", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(champ => champ.Key.ToLowerInvariant(), champ => champ.Value.GetString()?.Trim());

        var nombreChampsValides = champsSdi12.Values.Count(valeur => !string.IsNullOrWhiteSpace(valeur));
        var donneesCapteursBrutes = string.Join("\n", champsSdi12.Values.Where(valeur => !string.IsNullOrWhiteSpace(valeur)));
        var chargeUtileRecue = JsonSerializer.Serialize(chargeUtile, new JsonSerializerOptions { WriteIndented = true });

        return new ContexteIngestionMulticapteurWet150(
            endpoint,
            horodatageMesure,
            horodatageMesure.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            chargeUtileRecue,
            appareil,
            chargeUtile.DevEui,
            champsSdi12,
            nombreChampsValides,
            donneesCapteursBrutes);
    }

    private static void ValiderContexte(ContexteIngestionMulticapteurWet150 contexte)
    {
        SensorDataValidator.ValidateFormatOrThrow(
            contexte.DonneesCapteursBrutes,
            contexte.Endpoint,
            contexte.DevEui,
            contexte.NombreChampsValides,
            contexte.Appareil.Name,
            contexte.Appareil.Description,
            contexte.Appareil.CompanyName,
            contexte.ChargeUtileRecue);
    }
}
